#include <windows.h>
#include <stdio.h>
#include <evntprov.h>
#include <thread>
#include <chrono>

static const GUID ProviderGuid = 
{ 0x52345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF4 } };

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

class Cache
{
private:
    static int* staticData;
    static int staticSize;
    int* instanceData;
    
public:
    Cache()
    {
        staticSize += 10000;
        staticData = new int[staticSize];
        
        instanceData = new int[1000];
        
        for (int i = 0; i < staticSize; i++)
        {
            staticData[i] = i;
        }
        
        for (int i = 0; i < 1000; i++)
        {
            instanceData[i] = i;
        }
        
        volatile int sum = 0;
        for (int i = 0; i < staticSize; i++)
        {
            sum += staticData[i];
        }
        
        for (int i = 0; i < 1000; i++)
        {
            sum += instanceData[i];
        }
    }
    
    ~Cache()
    {
        delete[] instanceData;
    }
};

int* Cache::staticData = nullptr;
int Cache::staticSize = 0;

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 1000; i++)
    {
        Cache* cache = new Cache();
        delete cache;
        
        if (i % 100 == 0)
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

