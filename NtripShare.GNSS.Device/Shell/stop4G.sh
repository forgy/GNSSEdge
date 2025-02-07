#!/bin/bash
echo 271 > /sys/class/gpio/export
echo out > /sys/class/gpio/gpio271/direction
echo 1 > /sys/class/gpio/gpio271/value