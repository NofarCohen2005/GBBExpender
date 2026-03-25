int INTTCMaxDescriptor = 1;
int INTTCMaxMessage = 1;


enum  ExtendedGBBdescriptorName{
    Nofar = INTTCMaxDescriptor + 1,
};
    
enum  ExtendedGBBmessageName{
    NofarMessage = INTTCMaxMessage + 1,

    ExtendedMaxMessages = INTTCMaxMessage + 1000
};