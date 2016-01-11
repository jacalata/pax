from logging import debugPrint
import datetime #create timestamped filenames
import string #for clearing weird characters out of strings for filenames
import os


now = datetime.datetime.now()
paxEncoding = "utf-8" #is waht the pax site says they use

#extract file saving code, it gets used a lot in this
#data is the (string) data to save in the file
#filename is the filename to save it as
#extension is usually either html or xml
def saveStringAsFile(data, filename, DEBUGPRINT):
    safeFilename = filename
    if DEBUGPRINT:    
        print(safeFilename)
    fileWriter = open(safeFilename, 'w', encoding=paxEncoding)
    debugPrint(DEBUGPRINT, "stringAsFile", type(data))
    fileWriter.write(data)
    fileWriter.close()

def saveBytesAsFile(data, filename, DEBUGPRINT):
    safeFilename = filename
    print(safeFilename)
    fileWriter = open(safeFilename, 'w', encoding=paxEncoding)
    debugPrint(DEBUGPRINT, "bytesAsFile", type(data))
    fileWriter.write(data.decode())
    fileWriter.close()

def getOutputFolder(DEBUGPRINT):
    dateTimeInteger = now.strftime("%H%M%S")
    dateInteger = now.strftime("%Y%m%d")
    dirName = str(dateInteger+dateTimeInteger) # looks like 20151231100503 for dec 31 2015 at 10.05am and 3 seconds
    debugPrint(DEBUGPRINT, "output dir:", dirName)
    if not os.path.exists(dirName):
        os.makedirs(dirName)
    return dirName + "\\"
