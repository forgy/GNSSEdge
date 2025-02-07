#!/bin/bash
NAME='create_ap'
echo $NAME
ID=`ps -ef | grep "$NAME" | grep -v "$0" | grep -v "grep" | awk '{print $2}'`
echo $ID
echo "---------------"
for id in $ID
do
kill -9 $id
echo "killed $id"
done
echo "---------------"

echo 272 > /sys/class/gpio/export
echo out > /sys/class/gpio/gpio272/direction
echo 1 > /sys/class/gpio/gpio272/value