/**********************************************************
                       康威电子					 
功能：通过ADS1256，以15SPS(可配置为其他采样率)采集单端8路(可配置为差分4路)电压，
			并通过USB以虚拟串口形式返回PC，同时在驱动板液晶显示
接口：
      ADS1256模块    STM32开发板
      +5V   <------  5.0V      5V供电
      GND   -------  GND       地

      DRDY  ------>  PC3       准备就绪
      CS    <------  PC12      SPI_CS
      DIN   <------  PC10      SPI_MOSI
      DOUT  ------>  PC9       SPI_MISO
      SCLK  <------  PC11      SPI时钟
      GND   -------  GND       地
      PDWN  <------  PC8       掉电控制 常高
      RST   <------  PC13      复位信号 常高
时间：2015/11/3
版本：1.0
作者：康威电子
其他：
			更多电子需求，请到淘宝店，康威电子竭诚为您服务 ^_^
			https://kvdz.taobao.com/
			--USB驱动请从淘宝客服处获取。
**********************************************************/
#include "led.h"
#include "delay.h"
#include "sys.h"
#include "key.h"
#include "usart.h"
#include "exti.h"
#include "wdg.h"
#include "timer.h"
#include "stmflash.h"
#include "usb_lib.h"
#include "math.h"
#include "hw_config.h"
#include "string.h"
#include "stdlib.h"
#include "lcd.h"
#include "task_manage.h"
#include "bsp_ads1256.h"


int main(void)
 {
	int32_t adc[8];
	int32_t volt[8];
	uint8_t i;
	uint8_t ch_num;
	char infoBackPC[64];
	char infoShowLCD[64];
  SystemInit();
	delay_init(72);	     //延时初始化
	NVIC_Configuration();
	uart_init(256000);   //串口初始化
	key_init();//按键初始化
	initial_lcd();//液晶初始化
	LCD_Clear();
	LCD_Refresh_Gram();
	LCD_Show_CEStr(0,0,"ADS1256");//黑色
	LCD_Refresh_Gram();
	USBRelinkConfig();
	delay_ms(100);	/* 等待上电稳定，等基准电压电路稳定, bsp_InitADS1256() 内部会进行自校准 */
bsp_InitADS1256();	/* 初始化配置ADS1256.  PGA=1, DRATE=30KSPS, BUFEN=1, 输入正负5V */
	
	/* 打印芯片ID (通过读ID可以判断硬件接口是否正常) , 正常时状态寄存器的高4bit = 3 */
	{
		uint8_t id;

		id = ADS1256_ReadChipID();

		USB_SendStr("\r\n");
		USB_SendStr("读取芯片ID\r\n");
		if (id != 3)
		{
			sprintf(infoBackPC, "Error, ASD1256 Chip ID = 0x%X\r\n", id);
		}
		else
		{
			sprintf(infoBackPC, "Ok, ASD1256 Chip ID = 0x%X\r\n", id);
		}
		USB_TxWrite((uint8_t*)infoBackPC, strlen(infoBackPC));
	}

	/* 设置PGA增益，数据更新速率 */
	#if 1
		USB_SendStr("\r\nPGA增益 = 1, 数据输出速率 = 15sps, 单端8路扫描\r\n\r\n");

		ADS1256_CfgADC(ADS1256_GAIN_1, ADS1256_15SPS);	/* 配置ADC参数： 增益1:1, 数据输出速率 15Hz */

		/*
		   中断服务程序会自动读取ADC结果保存在全局变量，主程序通过 ADS1256_GetAdc() 函数来读取这些数据
		*/
		ADS1256_StartScan(0);	/* 启动中断扫描模式. 0表示单端8路，1表示差分4路 */
		ch_num = 8;		/* 通道数 = 8 或者4 */
	#else
		USB_SendStr("\r\nPGA增益 = 1, 数据输出速率 = 15sps, 差分4路扫描\r\n\r\n");

		ADS1256_CfgADC(ADS1256_GAIN_1, ADS1256_15SPS);	/* 配置ADC参数： 增益1:1, 数据输出速率 15Hz */

		/*
		   中断服务程序会自动读取ADC结果保存在全局变量，主程序通过 ADS1256_GetAdc() 函数来读取这些数据
		*/
		ADS1256_StartScan(1);	/* 启动中断扫描模式. 0表示单端8路，1表示差分4路 */
		ch_num = 4;		/* 通道数 = 8 或者4 */
	#endif
	
	while(1)
	{
		for (i = 0; i < ch_num; i++)
		{
			/* 从全局缓冲区读取采样结果。 采样结果是在中断服务程序中读取的。*/
			adc[i] = ADS1256_GetAdc(i);

			/* 4194303 = 2.5V , 这是理论值，实际可以根据2.5V基准的实际值进行公式矫正 */
			volt[i] = ((int64_t)adc[i] * 2500000) / 4194303;	/* 计算实际电压值（近似估算的），如需准确，请进行校准 */
		}
		if(upToPC)
		/* 打印采集数据 */
		{
			int32_t iTemp;
			upToPC = 0;
			sprintf(infoBackPC, "[%dCH_NUM]\r\n", ch_num);USB_TxWrite((uint8_t*)infoBackPC, strlen(infoBackPC));
			for (i = 0; i < ch_num; i++)
			{
				iTemp = volt[i];	/* 余数，uV  */
				if (iTemp < 0)
				{
					iTemp = -iTemp;
					sprintf(infoBackPC, "%d=%6d,(-%d.%03d %03d V) \r\n", i, adc[i], iTemp /1000000, (iTemp%1000000)/1000, iTemp%1000);
				}
				else
				{
					sprintf(infoBackPC,"%d=%6d,( %d.%03d %03d V) \r\n", i, adc[i], iTemp/1000000, (iTemp%1000000)/1000, iTemp%1000);
				}
				USB_TxWrite((uint8_t*)infoBackPC, strlen(infoBackPC));
			}
			//在液晶均显示8通道电压，单位0.1mv
			for(i = 0; i < 4; i++)
			{
				sprintf(infoShowLCD, "CH%d", i*2);
				LCD_ShowString(1,0 , i*16, (const u8*)infoShowLCD);
				LCD_Refresh_Gram();
				sprintf(infoShowLCD, "%05d", volt[i*2]/100);
				LCD_ShowString(0,24 , i*16, (const u8*)infoShowLCD);
				LCD_Refresh_Gram();

				sprintf(infoShowLCD, "CH%d", i*2+1);
				LCD_ShowString(1,64 , i*16, (const u8*)infoShowLCD);
				LCD_Refresh_Gram();
				sprintf(infoShowLCD, "%05d", volt[i*2+1]/100);
				LCD_ShowString(0,88 , i*16, (const u8*)infoShowLCD);
				LCD_Refresh_Gram();
			}
		LCD_Refresh_Gram();
		}
////		USB_TxWrite("你好啊！！\r\n", 12);
//		UartCmdGet();
	}
 }
