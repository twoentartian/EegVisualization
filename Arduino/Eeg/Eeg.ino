#include <SPI.h>
#include "ads12xx.h"

#define BUFFER_LENGTH 3

/**
 * SCK  -- SPI_1
 * DIN  -- SPI_4
 * DOUT -- SPI_0
 * RST  -- 3V3 (Start)
 * SYNC -- 3V3 (Start)
 * DRDY -- 24
 * CS   -- 21
 */

int  START = 26;							//引脚定义，对应ADS1256上两个常高电平的IO
int  CS = 21;									//片选
int  DRDY = 24;							//数据准备标志

ads12xx ADS;								//实例化ADS对象

void setup()
{
	Serial.begin(250000);				//打开串口 波特率：250000
	while (!Serial)							//当串口围初始化时等待
	{

	}
	ADS.begin(CS, START, DRDY);  //initialize ADS as object of the ads12xx class
	ADS.Reset();
	delay(10);									//延时10ms

	//ADS.SendCMD(SELFCAL);			//Preforming Self Gain and Offset Callibration
  //delay(5);
  //ADS.SendCMD(SYSOCAL);       //Preforming System Offset Callibration
	//delay(5);
	//ADS.SendCMD(SYSGCAL);       //Preforming System Gain Callibration
	//delay(5);
}

double volts[BUFFER_LENGTH];		//用于存储电压的数组，用于啸叫滤波

void loop()
{
	uint32_t dataUL = ADS.GetConversion();		//获取ADS的直接数据 24位
	int timer1 = micros();									//读取Arduino的计时器，单位us
	int32_t dataL;
	if (dataUL > 0x800000)									//如果首位为1，代表负数（补码），将数据转化为正数。
	{
		dataL = dataUL - 16777216;
	}
	else
	{
		dataL = dataUL;
	}
	double voltage = (4.9986 * dataL / 8388608);	//计算实际所对应的电压
	if (voltage < 0.0000001)									//如果电压太小，小于1uV则认为数据错误（SPI通信中，所有数据为0）
	{
		return;
	}

	double ave = 0;												//均值
	for (int i = 0; i < BUFFER_LENGTH; i++)
	{
		ave += volts[i];
	}
	ave = ave / BUFFER_LENGTH;							//计算buffer中缓存的电压的均值
	double diff = voltage - ave;								//计算当前值与均值误差
	if (diff < 0)
	{
		diff = -diff;
	}
	if (diff > ave * 2)												//如果误差过大则省略这个数据，当前倍率为2
	{
		return;
	}

	//将新数据放入缓存中
	for (int i = 0; i < BUFFER_LENGTH - 1; i++)
	{
		volts[i] = volts[i + 1];
	}
	volts[BUFFER_LENGTH - 1] = voltage;
	Serial.print(timer1);
	Serial.print(";");
	Serial.println(voltage, 7);
	delay(4);															//延时一定时间以防止数据发送过快导致串口无法及时发送
}
