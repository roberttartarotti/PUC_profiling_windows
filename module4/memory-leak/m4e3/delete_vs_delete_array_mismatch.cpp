#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0x22345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF1 } };

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

void ProcessData()
{
    int* numbers = new int[250000];
    
    LogEvent(L"Array allocated");
    
    for (int i = 0; i < 250000; i++)
    {
        numbers[i] = i * i;
    }
    
    volatile int sum = 0;
    for (int i = 0; i < 250000; i++)
    {
        sum += numbers[i];
    }
    
    delete numbers;
    
    LogEvent(L"Array freed");
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 5000; i++)
    {
        ProcessData();
        
        if (i % 500 == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(15));
        }
    }

    LogEvent(L"Processing completed");

    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

