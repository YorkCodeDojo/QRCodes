#!/bin/bash

python3 -m virtualenv ./venv
source ./venv/bin/activate
pip install -r requirements.txt
./main.py

