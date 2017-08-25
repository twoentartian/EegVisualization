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

int  START = 26;
int  CS = 21;
int  DRDY = 24;

ads12xx ADS;

void setup()
{
	Serial.begin(250000);
	while (!Serial)
	{
		
	}
	ADS.begin(CS, START, DRDY);  //initialize ADS as object of the ads12xx class
	ADS.Reset();
	delay(10);

	//ADS.SendCMD(SELFCAL);			//Preforming Self Gain and Offset Callibration
  //delay(5);
  //ADS.SendCMD(SYSOCAL);       //Preforming System Offset Callibration
	//delay(5);
	//ADS.SendCMD(SYSGCAL);       //Preforming System Gain Callibration
	//delay(5);
}

double volts[BUFFER_LENGTH];

void loop()
{
	uint32_t dataUL = ADS.GetConversion();
	int timer1 = micros();
  int32_t dataL;
  if (dataUL > 0x800000)
  {
    dataL = dataUL - 16777216;
  }
  else
  {
    dataL = dataUL;
  }
	double voltage = (4.9986 * dataL / 8388608);
  if(voltage < 0.0000001)
  {
    return;
  }
  
  double ave = 0;
  for (int i = 0; i < BUFFER_LENGTH; i++)
  {
    ave += volts[i];
  }
  ave = ave / BUFFER_LENGTH;
  double diff = voltage - ave;
  if (diff < 0)
  {
    diff = -diff;
  }
  if(diff > )
  {
    //return;
  }
  
  for (int i = 0; i < BUFFER_LENGTH-1; i++)
  {
    volts[i] = volts[i+1];
  }
  volts[BUFFER_LENGTH-1] = voltage;
  Serial.print(timer1);
  Serial.print(";");
  Serial.println(voltage,7);
  delay(4);
}
