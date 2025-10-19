#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

#define EVENT_MEMORY_LEAK 1
#define EVENT_OPERATION_START 2
#define EVENT_OPERATION_END 3

static const GUID ProviderGuid = 
{ 0x12345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 } };

REGHANDLE hProvider = 0;

void LogEvent(UCHAR level, USHORT eventId, PCWSTR message)
{
    if (hProvider)
    {
        EVENT_DATA_DESCRIPTOR eventData[1];
        EventDataDescCreate(&eventData[0], message, (ULONG)((wcslen(message) + 1) * sizeof(WCHAR)));
        EVENT_DESCRIPTOR descriptor = { eventId, 0, 0, level, 0, 0, 0 };
        EventWrite(hProvider, &descriptor, 1, eventData);
    }
}

class DataProcessor
{
private:
    int* data;
    int size;

public:
    DataProcessor(int s)
    {
        size = s;
        data = new int[size];
        wchar_t msg[256];
        swprintf_s(msg, 256, L"Data allocated", (size_t)(size * sizeof(int)));
        LogEvent(4, EVENT_MEMORY_LEAK, msg);
    }

    ~DataProcessor()
    {
    }

    void Process()
    {
        for (int i = 0; i < size; i++)
        {
            data[i] = i * 2;
        }
    }
};

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(4, EVENT_OPERATION_START, L"Processing started");

    for (int i = 0; i < 50000; i++)
    {
        DataProcessor* processor = new DataProcessor(25000);
        processor->Process();
        delete processor;
        
        if (i % 1000 == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }
    }

    LogEvent(4, EVENT_OPERATION_END, L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

