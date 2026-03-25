#include "Messages/F.h"
#include "Messages/RefactoredMessage.h"
#include "Messages/NewMessage.h"
#include "Messages/Nofar.h"

class MonitorNofarMessage : public GBBMonitor ::CMessageHandler {
public:
    MonitorNofarMessage(int id) : GBBMonitor::CMessageHandler(NofarMessage) {}
    
    virtual void FillData(char* pData)
    {
        HT::NofarMessage* pMessage = (HT::NofarMessage*)pData;
        AddFieldInt("Field1", pMessage->Field1);
    }
};
class MonitorNewMessage : public GBBMonitor::CMessageHandler {
public:
    MonitorNewMessage(int id) : GBBMonitor::CMessageHandler(NewMessage) {}

    virtual void FillData(char* pData)
    {
        HT::NewMessage* pMessage = (HT::NewMessage*)pData;
        AddFieldInt("FirstParam", pMessage->FirstParam);
    }
};


class MonitorRefactoredMessage : public GBBMonitor::CMessageHandler {
public:
    MonitorRefactoredMessage(int id) : GBBMonitor::CMessageHandler(RefactoredMessage) {}

    virtual void FillData(char* pData)
    {
        HT::RefactoredMessage* pMessage = (HT::RefactoredMessage*)pData;
        AddFieldInt("MaxField", pMessage->MaxField);
        AddFieldMETID("MinField", pMessage->MinField);
    }
};


class MonitorF : public GBBMonitor::CMessageHandler {
public:
    MonitorF(int id) : GBBMonitor::CMessageHandler(F) {}

    virtual void FillData(char* pData)
    {
        HT::F* pMessage = (HT::F*)pData;
        AddFieldInt("J", pMessage->J);
        AddFieldInt("H", pMessage->H);
        AddFieldInt("H", pMessage->H);
        AddFieldInt("", pMessage->);
    }
};
