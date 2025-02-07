#!/bin/bash
echo 272 > /sys/class/gpio/export
echo out > /sys/class/gpio/gpio272/direction
echo 0 > /sys/class/gpio/gpio272/value
