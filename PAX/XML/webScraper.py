#PAX7 script for scraping the web schedule and turning it into an xml doc

from urllib.request import urlopen, Request
from bs4 import BeautifulSoup
from xml.etree.ElementTree import Element, SubElement, Comment, tostring
import datetime
import sys
import getopt
import os
import string #for clearing weird characters out of strings for filenames
import time #sleep between requests

osPath = os.path.dirname(__file__)
<<<<<<< HEAD
generated_on = str(datetime.datetime.now())
year = datetime.datetime.now().year #doesn't get specified, I guess they expect you to know what year it is
=======
now = datetime.datetime.now()
generated_on = str(now)
year = now.year #doesn't get specified, I guess they expect you to know what year it is
>>>>>>> origin/East14
paxEncoding = "utf-8" #is waht the pax site says they use

currentSchedule = "http://aus.paxsite.com/schedule";

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

import string

class Del:
  def __init__(self, keep=string.ascii_letters+'\\'):
    self.comp = dict((ord(c),c) for c in keep)
  def __getitem__(self, k):
    return self.comp.get(k)

DD = Del()


#extract file saving code, it gets used a lot in this
#data is the (string) data to save in the file
#filename is the filename to save it as
#extension is usually either html or xml
def saveStringAsFile(data, filename, DEBUGMODE):
    safeFilename = filename#.translate(DD)
    print(safeFilename)
    fileWriter = open(safeFilename, 'w', encoding=paxEncoding)
    if DEBUGMODE:
        print(type(data))
    fileWriter.write(data)
    fileWriter.close()

def saveBytesAsFile(data, filename, DEBUGMODE):
    safeFilename = filename#.translate(DD)
    print(safeFilename)
    fileWriter = open(safeFilename, 'w', encoding=paxEncoding)
    if DEBUGMODE:
        print(type(data))
    fileWriter.write(data.decode())
    fileWriter.close()

def saveTextIOAsFile(data, filename, DEBUGMODE):
    safeFilename = filename.translate(DD)
    print(safeFilename)
    fileWriter = open(safeFilename, 'w')
    if DEBUGMODE:
        print(type(data))
    fileWriter.write(data.read())
    fileWriter.close()

def main(argv):
    print ("Arguments")
    print (argv)
    print("---------------------------------------------------")

    # set defaults
    TESTLOCALLY = False
    SHORTMODE = False
    DEBUGMODE = True
    DEBUGPRINT = False
    DOWNLOAD = False

    sampledatafolder = os.path.join(osPath,"sampledata")    
    offlinefolder =  os.path.join(osPath, "offline")
    print(sampledatafolder)

    try:                                
        opts, args = getopt.getopt(argv, "dhlsvz:", ["debug", "help", "local", "short", "verbose", "zdownload"])
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
        elif opt in ("-s", "--short"):
            SHORTMODE = True
        elif opt in ("-v", "--verbose"):
            DEBUGPRINT = True
        elif opt in ("-z", "--zdownload"):
            DOWNLOAD = True
    print ("Debug, Local, Short, Verbose = ", DEBUGMODE, TESTLOCALLY, SHORTMODE, DEBUGPRINT)               

    DEBUGMODE = True;
    if (TESTLOCALLY):
        if (DOWNLOAD):
            print("TestLocally")
        pageLocation = sampledatafolder+"\schedule.htm"
        page = open(pageLocation, encoding='utf-8')
    else:
        url = currentSchedule;
        if (DEBUGMODE):
            print(url)
        headers = { 'User-Agent' : 'Mozilla/5.0' }
        req = Request(url, None, headers);
        sock = urlopen(req);
        page = sock.read() #returns a bytes object
        #save the page for offline work or debugging
        if DOWNLOAD:
            saveBytesAsFile(page, offlinefolder+"\schedule.html", DEBUGMODE);
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
        if (True):
            print("############"+dayName)
        
        timeblocks=day.findAll('div', 'timeBlock')
        if SHORTMODE:
            timeblocks = [timeblocks[0]]
            
        for timeblock in timeblocks:
            
            events = timeblock.findAll('li',  recursive=True)
            if SHORTMODE:
                events = [events[0]]

            if (DEBUGPRINT):
                print(events)
            
            # for each event, define name,kind,location,datetime,end,description
            for eventInfo in events:
                time.sleep(3)
                # time format is Sunday 9/2/2013 10:00 am
                eventTime = dayDate + "/" + str(year) + " " + timeblock.find('h3', 'time').text
                if DEBUGPRINT:
                    print(eventTime)
                

                event = SubElement(schedule, 'Event')
                event.set('datetime', eventTime)
                if DEBUGPRINT:
                    print(eventInfo)
                # to get more details, go through to the url
                detailUrl = eventInfo.find('a')['href']
                if (TESTLOCALLY):
                    if DEBUGMODE:
                        print("TestLocally")
                    detailUrl = detailUrl.rsplit('/', 1)[1]
                    detailUrl = os.path.join(offlinefolder, detailUrl + ".html")
                    detailPage = open(detailUrl, encoding=paxEncoding)
                else:
                    

                    headers = { 'User-Agent' : 'Mozilla/5.0' }
                    req = Request(detailUrl, None, headers);
                    detailSock = urlopen(req);
                    detailPage = detailSock.read()
                    if DEBUGPRINT:
                        print(detailUrl)
                    if DOWNLOAD:
                        saveBytesAsFile(detailPage,
                                    os.path.join(offlinefolder, detailUrl+".html"),
                                    DEBUGMODE)    
                detailSoup = BeautifulSoup(detailPage)
                eventDetail = detailSoup.find('div', 'white')
            #   <div class="white">
    		#	    <h2>Friday Night Concerts</h2>
    		#	    <p>Come one, come all!&nbsp; It&#8217;s time for the PAX EAST 2013 CONCERT LINEUP! Friday night it&#8217;s VGO, Those Who Fight and Protomen!</p>			
    		#		<h4>Panelists:</h4>
    		#	    <p>PAX East Musical Guests</p>
    		#		<ul class="meta">
    		#	       	<li class="main"><a href="http://east.paxsite.com/schedule/category/main" title="Main Theatre">Main Theatre</a></li><li class="con"><a href="http://east.paxsite.com/schedule/category/con" title="Concerts">Concerts</a></li>
    		#	    	<li class="date">Friday 3/22 <strong>8:30PM - 1:30AM</strong></li>
    		#	    </ul>
    		#   </div>
                
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
            #   <ul class="meta">
		    #       <li class="main"><a href="http://east.paxsite.com/schedule/category/main" title="Main Theatre">Main Theatre</a></li>
		    #       <li class="con"><a href="http://east.paxsite.com/schedule/category/con" title="Concerts">Concerts</a></li> <--optional 
		    #	    <li class="date">Friday 3/22 <strong>8:30PM - 1:30AM</strong></li>
            #   </ul>          
                if DEBUGPRINT:
                    print("details = " + options.text)
                if (options.find('li', 'con')):
                    endTimeIndex = 2 # concert info gets shoehorned in front of the datetime info
                    print("CON")
                else:
                    endTimeIndex = 1
                endTimeText = options.findAll('li', recursive=True)[endTimeIndex].text#eeewwww 
                if DEBUGMODE:
                    print(endTimeText)
                endTime = endTimeText.split()[4]
                if DEBUGMODE:
                    print(endTime)
                # need to go from 1:30AM to 1.30 AM
                endTime = endTime.replace("AM", " AM")
                endTime = endTime.replace("PM", " PM")

                eventEnd = str(dayDate) + "/" + str(year) + " " + str(endTime)
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
                if 'Concerts' in eventName:
                    eventKind = "Show"
                elif ('Tournament' in eventLocation) || ('Tournament' in eventName):
                    eventKind = "Contest"
                elif 'Omegathon' in eventName:
                    eventKind = "Omegathon"
                if DEBUGPRINT:
                    print("kind = " + eventKind)
                event.set('kind', eventKind)

                if eventKind not in Kinds:
                    Kinds.append(eventKind)

        xml_string = tostring(root).decode('utf-8')
        if DEBUGMODE or DEBUGPRINT:
            print(xml_string)

        saveStringAsFile(xml_string, osPath+"\\"+dayName+".xml", DEBUGMODE)

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

    saveStringAsFile(xml_string, osPath+"\ScheduleValues.xml", DEBUGMODE)
    print("done")

        
if __name__ == "__main__":
    main(sys.argv[1:])

