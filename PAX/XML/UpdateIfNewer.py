### for each file in filelisting 
### diff with the matching filename under /schedule
## as soon as there is a difference, quit diffing 
### (if there was no difference, do nothing else)
### validate the xml of each file, as in, correct according to the Event schema
### read the version from latestVersionInfo.xml in /schedule
## send /schedule to schedule$version.zip
## update latestVersionInfo.xml to current date and version+1
## copy these files down into schedule


#TODO: import fails, formencode not found?
from formencode import xml_compare
# have to strip these or fromstring carps
xml1 = """    <?xml version='1.0' encoding='utf-8' standalone='yes'?>
    <Stats start="1275955200" end="1276041599"></Stats>"""
xml2 = """     <?xml version='1.0' encoding='utf-8' standalone='yes'?>
    <Stats start="1275955200" end="1276041599"></Stats>"""
xml3 = """ <?xml version='1.0' encoding='utf-8' standalone='yes'?>
    <Stats start="1275955200"></Stats>"""

from lxml import etree
tree1 = etree.fromstring(xml1.strip())
tree2 = etree.fromstring(xml2.strip())
tree3 = etree.fromstring(xml3.strip())

import sys
reporter = lambda x: sys.stdout.write(x + "\n")

assert xml_compare(tree1,tree2,reporter)
assert xml_compare(tree1,tree3,reporter) is False