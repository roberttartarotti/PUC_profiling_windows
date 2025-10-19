#include <windows.h>
#include <stdio.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xB2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFA } };

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

class Base
{
protected:
    int* baseData;
    
public:
    Base()
    {
        baseData = new int[100];
    }
    
    ~Base()
    {
        delete[] baseData;
    }
};

class Derived : public Base
{
private:
    char* derivedData;
    
public:
    Derived()
    {
        derivedData = new char[500];
    }
    
    ~Derived()
    {
        delete[] derivedData;
    }
};

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 500; i++)
    {
        Base* ptr = new Derived();
        delete ptr;
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

