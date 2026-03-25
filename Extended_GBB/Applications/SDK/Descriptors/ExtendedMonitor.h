#include "Descriptors/Nofar.h"

class MonitorNofar : public GBBMonitor ::CDescHandler {
public:
    MonitorNofar(int id) : GBBMonitor::CDescHandler(Nofar) {}
    
    virtual void FillData(char* pData)
    {
        HT::Nofar* pDesc = (HT::Nofar*)pData;
        AddFieldInt("Field1", pDesc->Field1);
    }
};