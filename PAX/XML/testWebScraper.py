import datetime
import sys
import getopt
import os


def main(argv):
    
    osPath = os.path.normpath(os.path.dirname(__file__) )
    print(osPath)
    webScraper = os.path.join(osPath, "webScraper.py")
    print (webScraper)
    
#test case 1: given a sample input, does it output valid xml containing a fully
#described Event
    os.system("python " + webScraper + " -ds --local .\sampledata\ > test1.out")
    diffValue = -1
    diffCommand = "cmp --silent sampledata\\test1expected.out test1.out"
    print(diffCommand)
    diffValue = os.system(diffCommand)
    if not (diffValue == 0):
        print ("Test Case FAILED!")
        print("test case 1: diff with expected log was not the same")

#buuuuut....my diff always fails because of the timestamps embedded in the files :(
        
    

#test case 2: given a sample input, does it output all the expected files
#(start by expecting friday saturday and sunday)
    #find day names in output log by looking for ############

#test case 3: given a sample input, does it exactly match the expected
# output files

#test case 3: given the website, does it output a file containing at least
# one event with all fields completed



if __name__ == "__main__":
    main(sys.argv[1:])
