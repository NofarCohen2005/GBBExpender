#include "Descriptors/Nofar.h"

#include "Messages/NofarMessage.h"

public ExtendedSizeDLL(){
    ADD_DESC1(Nofar);

    ADD_MESSAGE("NofarMessage", NofarMessage, NofarMessage);
}