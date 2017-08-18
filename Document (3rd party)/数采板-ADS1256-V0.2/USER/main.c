/**********************************************************
                       ��������					 
���ܣ�ͨ��ADS1256����15SPS(������Ϊ����������)�ɼ�����8·(������Ϊ���4·)��ѹ��
			��ͨ��USB�����⴮����ʽ����PC��ͬʱ��������Һ����ʾ
�ӿڣ�
      ADS1256ģ��    STM32������
      +5V   <------  5.0V      5V����
      GND   -------  GND       ��

      DRDY  ------>  PC3       ׼������
      CS    <------  PC12      SPI_CS
      DIN   <------  PC10      SPI_MOSI
      DOUT  ------>  PC9       SPI_MISO
      SCLK  <------  PC11      SPIʱ��
      GND   -------  GND       ��
      PDWN  <------  PC8       ������� ����
      RST   <------  PC13      ��λ�ź� ����
ʱ�䣺2015/11/3
�汾��1.0
���ߣ���������
������
			������������뵽�Ա��꣬�������ӽ߳�Ϊ������ ^_^
			https://kvdz.taobao.com/
			--USB��������Ա��ͷ�����ȡ��
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
	delay_init(72);	     //��ʱ��ʼ��
	NVIC_Configuration();
	uart_init(256000);   //���ڳ�ʼ��
	key_init();//������ʼ��
	initial_lcd();//Һ����ʼ��
	LCD_Clear();
	LCD_Refresh_Gram();
	LCD_Show_CEStr(0,0,"ADS1256");//��ɫ
	LCD_Refresh_Gram();
	USBRelinkConfig();
	delay_ms(100);	/* �ȴ��ϵ��ȶ����Ȼ�׼��ѹ��·�ȶ�, bsp_InitADS1256() �ڲ��������У׼ */
bsp_InitADS1256();	/* ��ʼ������ADS1256.  PGA=1, DRATE=30KSPS, BUFEN=1, ��������5V */
	
	/* ��ӡоƬID (ͨ����ID�����ж�Ӳ���ӿ��Ƿ�����) , ����ʱ״̬�Ĵ����ĸ�4bit = 3 */
	{
		uint8_t id;

		id = ADS1256_ReadChipID();

		USB_SendStr("\r\n");
		USB_SendStr("��ȡоƬID\r\n");
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

	/* ����PGA���棬���ݸ������� */
	#if 1
		USB_SendStr("\r\nPGA���� = 1, ����������� = 15sps, ����8·ɨ��\r\n\r\n");

		ADS1256_CfgADC(ADS1256_GAIN_1, ADS1256_15SPS);	/* ����ADC������ ����1:1, ����������� 15Hz */

		/*
		   �жϷ��������Զ���ȡADC���������ȫ�ֱ�����������ͨ�� ADS1256_GetAdc() ��������ȡ��Щ����
		*/
		ADS1256_StartScan(0);	/* �����ж�ɨ��ģʽ. 0��ʾ����8·��1��ʾ���4· */
		ch_num = 8;		/* ͨ���� = 8 ����4 */
	#else
		USB_SendStr("\r\nPGA���� = 1, ����������� = 15sps, ���4·ɨ��\r\n\r\n");

		ADS1256_CfgADC(ADS1256_GAIN_1, ADS1256_15SPS);	/* ����ADC������ ����1:1, ����������� 15Hz */

		/*
		   �жϷ��������Զ���ȡADC���������ȫ�ֱ�����������ͨ�� ADS1256_GetAdc() ��������ȡ��Щ����
		*/
		ADS1256_StartScan(1);	/* �����ж�ɨ��ģʽ. 0��ʾ����8·��1��ʾ���4· */
		ch_num = 4;		/* ͨ���� = 8 ����4 */
	#endif
	
	while(1)
	{
		for (i = 0; i < ch_num; i++)
		{
			/* ��ȫ�ֻ�������ȡ��������� ������������жϷ�������ж�ȡ�ġ�*/
			adc[i] = ADS1256_GetAdc(i);

			/* 4194303 = 2.5V , ��������ֵ��ʵ�ʿ��Ը���2.5V��׼��ʵ��ֵ���й�ʽ���� */
			volt[i] = ((int64_t)adc[i] * 2500000) / 4194303;	/* ����ʵ�ʵ�ѹֵ�����ƹ���ģ�������׼ȷ�������У׼ */
		}
		if(upToPC)
		/* ��ӡ�ɼ����� */
		{
			int32_t iTemp;
			upToPC = 0;
			sprintf(infoBackPC, "[%dCH_NUM]\r\n", ch_num);USB_TxWrite((uint8_t*)infoBackPC, strlen(infoBackPC));
			for (i = 0; i < ch_num; i++)
			{
				iTemp = volt[i];	/* ������uV  */
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
			//��Һ������ʾ8ͨ����ѹ����λ0.1mv
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
////		USB_TxWrite("��ð�����\r\n", 12);
//		UartCmdGet();
	}
 }
