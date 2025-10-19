#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <evntprov.h>

static const GUID ProviderGuid = 
{ 0xA2345678, 0x1234, 0x1234, { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF9 } };

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

class String
{
private:
    char* data;
    int length;
    
public:
    String(const char* str)
    {
        length = strlen(str);
        data = new char[length + 1];
        strcpy_s(data, length + 1, str);
    }
    
    ~String()
    {
        delete[] data;
    }
    
    String(const String& other)
    {
        data = other.data;
        length = other.length;
    }
};

int main()
{
    EventRegister(&ProviderGuid, NULL, NULL, &hProvider);
    LogEvent(L"Processing started");

    for (int i = 0; i < 700; i++)
    {
        String original("This is a test string for memory leak demonstration");
        String copy = original;
    }

    LogEvent(L"Processing completed");
    EventUnregister(hProvider);

    printf("\nPress Enter to exit...");
    getchar();
    return 0;
}

