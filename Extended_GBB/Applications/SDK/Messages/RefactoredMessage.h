#pragma once
#include "Descriptors/MonitorUtilDef.h"
#include "GeneralTypes.h"
#include "AppObjectDefs.h"
#include "Inc/AppObjectDefs.h"

namespace HT {
    struct RefactoredMessage {
        int MaxField;
        unsigned int MinField;

        RefactoredMessage()
            : MaxField(INT_MAX), MinField(0)
        {
        }
    };
}
