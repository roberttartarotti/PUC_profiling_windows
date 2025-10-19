#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0x72345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF6 } };

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

class ResourceManager
{
private:
    char* resource;
    
public:
    ResourceManager()
    {
        resource = nullptr;
    }
    
    void Initialize()
    {
        resource = new char[500];
    }
    
    void Reinitialize()
    {
        resource = new char[500];
    }
    
    ~ResourceManager()
    {
        if (resource)
        {
            delete[] resource;
        }
    }
};

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 600; i++)
    {
        ResourceManager* mgr = new ResourceManager();
        mgr->Initialize();
        mgr->Reinitialize();
        delete mgr;
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

