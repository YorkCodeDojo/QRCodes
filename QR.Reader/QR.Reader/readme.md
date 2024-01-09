# Decoding QR Codes

## Positioning Markers
The three big squares (top left,  top right and bottom left) are for positioning the QR code.
To make our life easier we are going ignore these,  as our image is already correctly orientated.


## Step 1 - Read Image
Convert the `dojo.png` into a grid of black and white modules.
Each module is 8x8 pixels.

If you want to skip this step,  then the file `raw.txt` can be used.

## Step 2 - Version
Work out the version of the QR code using this formula:

`version = (width - 17) / 4`

## Step 3 - Format Information
The format information can be found in the line below the top left positioning marker.
Process is:
* Read the first 5 modules from the line
* Black means 1 and white means 0.  (ie.  Black Black White White White is 11000)
* XOR the value with .....
* The last 3 bits gives us the Id of the mask we need to apply.

EXPLAIN THE MASK HERE

## Step 4 - Read the encoding information
The 2x2 in the bottom right hand corner contains the encoding used in this QR code.
The bits should be read in the following order:
43
21
When reading the bits,  don't forget to invert any modules as specified by your mask.
Look up the encoding from the table below.

ADD TABLE

(Hint:  You should get Byte)


## Step 5 - Read the length
The length of the message can be found in the byte (8 modules) above the encoding square.  These
should be read in the following order.
87
65
43
21

## Step 6 - Read the message
Now we know the version, the mask, the encoding and the message length we can read the actual message.
Keep reading the bits as shown on the diagram below.


## Step 7 - Create your own message
Using the web site,  create your own QR code.  Make sure you set the version to 1.  Can you decode the message?
Don't forget you might need to apply a different mask.

## Step 8- Investigate how the error correction works.
Most of the remainder of the message contains some error correction information.  Look up how this works.






Resources

https://www.nayuki.io/page/qr-code-generator-library
https://www.researchgate.net/figure/The-structure-of-QR-code-in-version-2-M_fig5_333611998#:~:text=The%20QR%20structure%20(version%202,error%20correction%20codewords%20%5B31%5D%20.

https://blog.qartis.com/decoding-small-qr-codes-by-hand/

https://barcode.tec-it.com/en/MicroQR?data=YorkCodeDojo
https://www.onbarcode.com/micro_qr_code/