#include <SPI.h>
#include "ads12xx.h"

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
	Serial.begin(115200);
	while (!Serial)
	{
		
	}
	ADS.begin(CS, START, DRDY);  //initialize ADS as object of the ads12xx class
	ADS.Reset();
	delay(10);

	//ADS.SendCMD(SELFCAL);			//Preforming Self Gain and Offset Callibration
  delay(5);
  ADS.SendCMD(SYSOCAL);       //Preforming System Offset Callibration
	delay(5);
	ADS.SendCMD(SYSGCAL);       //Preforming System Gain Callibration
	delay(5);
}

void loop()
{
	uint32_t dataUL = ADS.GetConversion();
	int timer1 = micros();
  long dataL;
  if (long minus = dataUL >> 23 == 1)
  {
    dataL = dataUL - 16777216;
  }
	Serial.println(dataL);
	double voltage = (4.9986 / 8388608)*dataL;
	Serial.println(voltage,10);
	delay(500);
}
