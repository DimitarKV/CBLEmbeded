#include <Arduino.h>
#include <../lib/modbus_connector/modbus_connector.h>
#include <../lib/color_sensor/color_sensor.h>
#include <../lib/display_interface/display_interface.h>
#include <Adafruit_ST7789.h>
#include <Adafruit_GFX.h>
#include <../lib/servo_shield/servo_controller.h>
#include <../lib/motor_driver/motor_driver.h>

#define STEPPER_IN1 5
#define STEPPER_IN2 6
#define STEPPER_IN3 7
#define STEPPER_IN4 3

ModbusConnector connector;
Display display = Display(10, 2, 4);
ColorSensor colorSensor;
ServoController servoController = ServoController();
MotorDriver motorDriver(STEPPER_IN1, STEPPER_IN3, STEPPER_IN2, STEPPER_IN4);

void writeToDisplay(ModbusPacket inputPacket)
{
  display.interpretMessage((char *)inputPacket.data);
}

void readColorSensor(ModbusPacket packet) {
  ColorSensorData data = colorSensor.getData();
  connector.sendData(packet.function, (byte*)(&data), 12);
}

void setServoAngle(ModbusPacket packet) {
  if(packet.dataLength % 2 == 0) {
    for (int i = 0; i < packet.dataLength / 2; i++)
    {
      servoController.setImmediateAngle(packet.data[2 * i], packet.data[2 * i + 1]);
    }
  }
}

void moveBelt(ModbusPacket packet) {
  uint16_t *distance = (uint16_t*)(&packet.data);
  motorDriver.moveLength(*distance);
}

void setup() {
  Serial.begin(1000000);
  
  display.init(135, 240, 3);
  
  colorSensor.init();

  motorDriver.init();
  // motorDriver.moveSteps(1000);

  servoController.init();
  servoController.addServo(0, 0);
  servoController.addServo(1, 0);
  servoController.addServo(2, 100);
  servoController.addServo(3, 0);
  servoController.addServo(4, 0);
  
  connector.addProcessor(0, *writeToDisplay);
  connector.addProcessor(1, *readColorSensor);
  connector.addProcessor(2, *moveBelt);
  connector.addProcessor(3, *setServoAngle);
}

void loop() {
  connector.tick();
  colorSensor.tick();
  servoController.tick();
  motorDriver.tick();
  // colorSensor.print();
}