#include <windows.h>
#include <stdio.h>
#include <vector>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xD2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFC } };

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

class Data
{
public:
    char* buffer;
    
    Data()
    {
        buffer = new char[200];
    }
};

void UseContainer()
{
    std::vector<Data*> dataVector;
    
    for (int i = 0; i < 50; i++)
    {
        dataVector.push_back(new Data());
    }
}

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 400; i++)
    {
        UseContainer();
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

