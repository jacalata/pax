#PAX7 script for scraping the web schedule and turning it into an xml doc

from urllib.request import urlopen
from bs4 import BeautifulSoup
from xml.etree.ElementTree import Element, SubElement, Comment, tostring
import datetime
import sys
import getopt
import os

osPath = os.path.dirname(__file__)
generated_on = str(datetime.datetime.now())
year = "2013" #doesn't get specified, I guess they expect you to know what year it is

# pull the metadata straight out of the schedule where possible
Locations = []
Kinds = [] #"Tabletop", "Panel", "Show", "Contest", "Omegathon", "FreePlay", "D&D", "Social"]

def usage():
    print("open a command line and call python webScraper.py.")
    print("arguments:")
    print("d / --debug: include basic logging")
    print("l / --local: test without hitting the server. Must provide local input")
    print("s / --short: parse only the first event of each day, speed up test cycle")
    print("v / --verbose: spew verbose logs for debugging")


def main(argv):
    print ("Arguments")
    print (argv)
    print("---------------------------------------------------")

    # set defaults
    TESTLOCALLY = True
    SHORTMODE = True
    DEBUGMODE = False
    DEBUGPRINT = False
    sampledatafolder = os.path.join(osPath,"sampledata") 

    try:                                
        opts, args = getopt.getopt(argv, "dhlsv:", ["debug", "help", "local=", "short", "verbose"])
    except getopt.GetoptError:          
        usage()                         
        sys.exit(2)                     
    for opt, arg in opts:               
        if opt in ("-d", "--debug"):
            DEBUGMODE = True
        elif opt in ("-h", "--help"):
            usage()
            sys.exit()
        elif opt in ("-l", "--local"):
            TESTLOCALLY = True
            sampledatafolder = arg    
        elif opt in ("-s", "--short"):
            SHORTMODE = True
        elif opt in ("-v", "--verbose"):
            DEBUGPRINT = True
    print ("Debug, Local, Short, Verbose = ", DEBUGMODE, TESTLOCALLY, SHORTMODE, DEBUGPRINT)               

    
    if (TESTLOCALLY):
        if (DEBUGMODE):
            print("TestLocally")  
        page = open(sampledatafolder+"\schedule.htm", encoding='utf-8')
    else:
        url = "http://prime.paxsite.com/schedule"
        if (DEBUGMODE):
            print(url)
        sock = urlopen(url)
        page = sock.read()
        sock.close()

    soup = BeautifulSoup(page)

    schedule = soup.find('ul', attrs={'id': 'schedule'}) 
    days = schedule.findAll('li', recursive=False) #list of 3 days 
    for day in days:

        root = Element('xml')
        root.set('version', '1.0')
        if not SHORTMODE:
            root.append(Comment('Generated by ElementTree in webScraper.py at ' + generated_on))
            #makes life easier if we generate it without a timestamp for super basic testing
        schedule = SubElement(root, 'Schedule')
        
        dayName = day['id']
        dayDate = day.find('h2', recursive=True).text 
        if (DEBUGMODE):
            print("############"+dayName)
        
        timeblocks=day.findAll('div', 'timeBlock')
        if DEBUGMODE:
            timeblocks = [timeblocks[0]]
            
        for timeblock in timeblocks:
            
            events = timeblock.findAll('li',  recursive=True)
            if DEBUGMODE:
                events = [events[0]]

            if (DEBUGPRINT):
                print(events)
            
            # for each event, define name,kind,location,datetime,end,description
            for eventInfo in events:
                # time format is Sunday 9/2/2013 10:00 am
                eventTime = dayDate + "/" + year + " " + timeblock.find('h3', 'time').text
                if DEBUGPRINT:
                    print(eventTime)
                

                event = SubElement(schedule, 'Event')
                event.set('datetime', eventTime)
                if DEBUGPRINT:
                    print(eventInfo)
                # to get more details, go through to the url
                if (TESTLOCALLY):
                    if DEBUGMODE:
                        print("TestLocally")
                    if 's' in dayName:
                        detailUrl = sampledatafolder + "\eventDetail.htm"
                    else:
                        detailUrl = sampledatafolder + "\eventDetail2.htm"
                    detailPage = open(detailUrl, encoding='utf-8')
                else:
                    detailUrl = eventInfo.find('a')['href']
                    detailPage = urlopen(detailUrl)
                    if DEBUGPRINT:
                        print(detailUrl)
                    
                detailSoup = BeautifulSoup(detailPage)
                eventDetail = detailSoup.find('div', 'white')
                
                eventName=eventDetail.find('h2').text
                if DEBUGPRINT:
                    print("Name = " + eventName)
                event.set('name', eventName)
                
                details = eventDetail.findAll('p')
                if (len(details) == 0):
                    eventDescription = ""
                else:
                     eventDescription = details[0].text
                if eventDetail.find('h4') is not None:
                    eventDescription = eventDescription + " -- " + eventDetail.find('h4').text + " " + details[1].text
                if DEBUGPRINT:
                    print("description = " + eventDescription)
                event.set('description', eventDescription)
                
                options = eventDetail.find('ul', 'meta')
                eventEnd = dayDate + "/" + year + " " + options.findAll('li', recursive=True)[1].text.split()[4] #eeewwww
                if DEBUGPRINT:
                    print("end = " + eventEnd)
                event.set('end', eventEnd)

                eventLocation = options.find('a', recursive=True).text
                if DEBUGPRINT:
                    print("location= " + eventLocation)
                event.set('location', eventLocation)

                if eventLocation not in Locations:
                    Locations.append(eventLocation)
                
                eventKind="Panel" #default assumption. Special cases:
                if 'concerts' in eventName:
                    eventKind = "Show"
                elif 'tourney' in eventLocation:
                    eventKind = "Contest"
                elif 'omegathon' in eventName:
                    eventKind = "Omegathon"
                if DEBUGPRINT:
                    print("kind = " + eventKind)
                event.set('kind', eventKind)

                if eventKind not in Kinds:
                    Kinds.append(eventKind)

        xml_string = tostring(root).decode('utf-8')
        if DEBUGMODE or DEBUGPRINT:
            print(xml_string)

        filename = osPath + dayName + ".xml"
        fileWriter = open(filename, 'w')
        fileWriter.write(xml_string)
        fileWriter.close()

    # print xml schema showing locations and kinds of events
    root = Element('xml')
    root.set('version', '1.0')
    root.append(Comment('Generated by ElementTree in webScraper.py at ' + generated_on))

    xml_locations = SubElement(root, 'Locations')
    for name in Locations:
        location = SubElement(xml_locations, 'location')
        location.set('name', name)

    xml_kinds = SubElement(root, 'Kinds')
    for name in Kinds:
        kind = SubElement(xml_kinds, 'kind')
        kind.set('name', name)

    xml_string = tostring(root).decode('utf-8')
    if DEBUGMODE or DEBUGPRINT:
        print(xml_string)

    filename = osPath + "ScheduleValues.xml"
    fileWriter = open(filename, 'w')
    fileWriter.write(xml_string)
    fileWriter.close()

    print("done")

        
if __name__ == "__main__":
    main(sys.argv[1:])
