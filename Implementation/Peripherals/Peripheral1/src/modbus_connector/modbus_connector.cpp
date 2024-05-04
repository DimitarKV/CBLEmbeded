#include "modbus_connector.h"


int ModbusConnector::extractNullTerminatedLength(char* buffer) {
    int index = 0;
    while (buffer[index] != '\0')
    {
        ++index;
    }
    return index;
}

byte ModbusConnector::calculateLRC(char* buffer)
{
    int calculatedLrc = 0;
    int index = 0;
    while (buffer[index] != '\0')
    {
        calculatedLrc = (calculatedLrc + buffer[index]) & 0xFF;
        index++;
    }
    calculatedLrc = (((calculatedLrc ^ 0xFF) + 1) & 0xFF);
    return calculatedLrc;
}

byte ModbusConnector::calculateLRC(char* buffer, int length)
{
    int calculatedLrc = 0;
    for (int i = 0; i < length; i++)
    {
        calculatedLrc = (calculatedLrc + buffer[i]) & 0xFF;
    }
    calculatedLrc = (((calculatedLrc ^ 0xFF) + 1) & 0xFF);
    return calculatedLrc;
}


bool ModbusConnector::serialRead()
{
    if (Serial.available())
    {
        serialBuffer[serialBufferIndex] = Serial.read();
        serialBufferIndex++;
        if (serialBufferIndex > 1 && serialBuffer[serialBufferIndex - 2] == '\r' && serialBuffer[serialBufferIndex - 1] == '\n')
        {
            serialBuffer[serialBufferIndex - 2] = '\0';
            return true;
        }
    }
    return false;
}

void ModbusConnector::decodeModbusMessage(char* buffer)
{
    this->packet.isValid = false;
    int length = extractNullTerminatedLength(buffer);
    if (length >= 5 && buffer[0] == ':')
    {
        int dataLength = length - 5;
        char function[3];
        memcpy(function, &buffer[1], 2);
        function[2] = '\0';

        char data[dataLength + 1];
        memcpy(data, &buffer[3], dataLength);
        data[dataLength] = '\0';

        char lrc[3];
        memcpy(lrc, &buffer[length - 2], 2);
        lrc[2] = '\0';

        int receivedLRC = std::strtol(lrc, 0, 16);
        int calculatedLRC = calculateLRC(&buffer[1], dataLength + 2);
        if (receivedLRC != calculatedLRC)
        {
            return;
        }

        this->packet.isValid = true;
        this->packet.function = std::strtol(function, 0, 16);
        this->packet.dataLength = dataLength / 2;
        for (int i = 0; i < dataLength; i += 2)
        {
            char dataByte[3];
            memcpy(dataByte, &data[i], 2);
            dataByte[2] = '\0';
            this->packet.data[i / 2] = std::strtol(dataByte, 0, 16);
        }
    }
}

// ModbusPacket Communication::decodeModbusMessage(byte* buffer, int length)
// {
//     this->packet.function = 0;
//     this->packet.data = nullptr;
//     this->packet.isValid = false;
//     if (input.length() >= 7 && input[0] == ':')
//     {
//         std::string function = input.substr(1, 2);
//         std::string data = input.substr(3, input.length() - 7);
//         std::string lrc = input.substr(input.length() - 4, 2);
//         std::string end = input.substr(input.length() - 2);
//         int receivedLRC = std::strtol(lrc.c_str(), 0, 16);
//         int calculatedLRC = calculateLRC(function + data);
//         if (receivedLRC != calculatedLRC)
//         {
//             return packet;
//         }
//         this->packet.isValid = true;
//         this->packet.function = std::strtol(function.c_str(), 0, 16);
//         this->packet.dataLength = data.length() / 2;
//         this->packet.data = new byte[this->packet.dataLength];
//         for (int i = 0; i < data.length(); i += 2)
//         {
//             this->packet.data[i / 2] = std::strtol(data.substr(i, 2).c_str(), 0, 16);
//         }
//     }
//     return this->packet;
// }

void ModbusConnector::processModbusCommand(ModbusPacket packet) {
    if(packet.isValid) {
        this->processors[packet.function](packet);
    }
}

void ModbusConnector::handleSerial()
{
    decodeModbusMessage(serialBuffer);
    
    if(packet.isValid) {
        processModbusCommand(packet);
    }
    serialBufferIndex = 0;

    // Convert the buffer to a usable std::string
    // if(token[0] == ':') {
    //   serialBufferIndex = 0;
    //   return;
    // }
    // // Extract the command
    // std::string command = token.substr(0, 3);
    // // Extract the parameters
    // std::string parameters = token.substr(3);

    // if (command.compare("WDN") == 0)
    // {
    //   writeToDisplayNoScrolling(parameters);
    // } else if(command.compare("SSP") == 0) {
    //   //std::vector<std::string> parsedParameters = splitParameters(parameters);

    // }
}

void ModbusConnector::tick()
{
    if (serialRead())
    {
        handleSerial();
    }
}

void ModbusConnector::addProcessor(byte function, modbusFuncPtr processor) {
    processors[function] = processor;
}