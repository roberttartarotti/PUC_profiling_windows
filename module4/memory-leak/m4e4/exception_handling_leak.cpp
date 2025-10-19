#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0x32345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF2 } };

REGHANDLE hProvider = 0;

void LogEvent(PCWSTR message)
{
    if (hProvider)
    {
        EVENT_DATA_DESCRIPTOR eventData[1];
        EventDataDescCreate(&eventData[0], message, (ULONG)((wcslen(message) + 1) * sizeof(WCHAR)));
        EVENT_DESCRIPTOR descriptor = { 1, 0, 0, 4, 0, 0, 0 };
        EventWrite(hProvider, &descriptor, 1, eventData);
    }
}

void RiskyOperation(int value)
{
    char* buffer = new char[100000];
    
    for (int i = 0; i < 100000; i++)
    {
        buffer[i] = (char)(i % 256);
    }
    
    volatile int sum = 0;
    for (int i = 0; i < 100000; i++)
    {
        sum += buffer[i];
    }
    
    if (value < 0)
    {
        throw "Invalid value";
    }
    
    sprintf_s(buffer, 100000, "Value: %d", value);
    delete[] buffer;
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 50000; i++)
    {
        try
        {
            RiskyOperation(i % 10 - 5);
        }
        catch (...)
        {
        }
        
        if (i % 5000 == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(20));
        }
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

