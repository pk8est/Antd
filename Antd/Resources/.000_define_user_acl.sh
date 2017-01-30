#!/bin/bash

IFS=$'\n'
T=$'\t'

USERTOGET=$1
#echo $USERTOGET

### VARS START ###
USERHOMEPATH=/Data/UserData/Home
USERRELHOMEPATH=../Home
USERSCAMBIOPATH=/Data/UserData/Scambio
SELECTEDNAME=$USERTOGET
### VARS STOP ###

COMMAND=`wbinfo -u|grep $USERTOGET`
#echo $COMMAND
for OUTPUT in $COMMAND
do
#echo $OUTPUT
CYCLEONE=`wbinfo -n $OUTPUT |awk '{print $1}'`
        for OUTPUT in $CYCLEONE
        do
        #echo $OUTPUT
        CYCLETWO=`wbinfo -S $CYCLEONE |awk '{print $1}'`
        SELECTEDUID=$CYCLETWO
        echo $CYCLETWO
        done
done

### VARS RECAP START###
echo USERHOMEPATH=$USERHOMEPATH
echo USERRELHOMEPATH=$USERRELHOMEPATH
echo USERSCAMBIOPATH=$USERSCAMBIOPATH
echo SELECTEDNAME=$SELECTEDNAME
echo SELECTEDUID=$SELECTEDUID
### VARS RECAP STOP ###

### PRE EXEC START ###
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Shared
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Saved\ Games
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Music
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Links
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Start\ Menu
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Desktop
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Pictures
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Contacts
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Videos
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Music
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Searches
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Documents
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Videos
echo mkdir -p $USERHOMEPATH/$SELECTEDNAME/Favorites
echo cp -f .010_Home_SKEL.acl 010_Home_$SELECTEDNAME.acl
echo cp -f .011_Shared_SKEL.acl 011_Shared_$SELECTEDNAME.acl
echo replace VALUETOREPLACE $SELECTEDUID -- 010_Home_$SELECTEDNAME.acl
echo replace VALUETOREPLACE $SELECTEDUID -- 011_Shared_$SELECTEDNAME.acl
echo setfacl --set-file=010_Home_$SELECTEDNAME.acl -R /Data/UserData/Home/$SELECTEDNAME
echo setfacl --set-file=011_Shared_$SELECTEDNAME.acl -R /Data/UserData/Home/$SELECTEDNAME/Shared
echo ln -s $USERRELHOMEPATH/$SELECTEDNAME/Shared $USERSCAMBIOPATH/$SELECTEDNAME"_Shared"
### PRE EXEC STOP ###
### EXEC START ###
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Shared
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Saved\ Games
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Music
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Links
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Start\ Menu
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Desktop
mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Pictures
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Contacts
mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Videos
mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Music
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Searches
mkdir -p $USERHOMEPATH/$SELECTEDNAME/My\ Documents
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Videos
mkdir -p $USERHOMEPATH/$SELECTEDNAME/Favorites
cp -f .010_Home_SKEL.acl 010_Home_$SELECTEDNAME.acl
cp -f .011_Shared_SKEL.acl 011_Shared_$SELECTEDNAME.acl
replace VALUETOREPLACE $SELECTEDUID -- 010_Home_$SELECTEDNAME.acl
replace VALUETOREPLACE $SELECTEDUID -- 011_Shared_$SELECTEDNAME.acl
setfacl --set-file=010_Home_$SELECTEDNAME.acl -R /Data/UserData/Home/$SELECTEDNAME
setfacl --set-file=011_Shared_$SELECTEDNAME.acl -R /Data/UserData/Home/$SELECTEDNAME/Shared
ln -s $USERRELHOMEPATH/$SELECTEDNAME/Shared $USERSCAMBIOPATH/$SELECTEDNAME"_Shared"
### FOR EMPTY CLEANUP ###
rmdir $USERHOMEPATH/* 2> /dev/null
### EXEC STOP ###

#STARTPATH="/Data/UserData/Home/"
#wbinfo -n m.zafferana
#wbinfo -S S-1-5-21-1191849564-1695385468-1789397799-1773