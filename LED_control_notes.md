# Quick notes on my approach to controlling the Virpil device LEDs

First and foremost, I don't own a lot of Virpil devices, so the findings given here only cover the devices I have available to me.

The test hardware used was the Virpil Rotor TCS Base with the Hawk-60 grip and a Virpil Control Panel #1 acting in slave and standalone mode during testing. Any assumptions around hardware behaviour is based on these devices. I backed the Vector Flight Yoke base so I will try and include that in my testing if Virpil ever manage to ship it.

I had originally copied a solution from a 3rd party project, [Virpil-Communicator](https://github.com/charliefoxtwo/Virpil-Communicator), but the provided logic for building data packages did not play nice with slaved devices and I never managed to get it working.

Instead I used Wireshark with the USBPCap plugin to sniff the USB traffic from the VPC_LED_Control.exe helper application and reverse engineered the data packets. This actually works out simpler than the solution provided in [Virpil-Communicator](https://github.com/charliefoxtwo/Virpil-Communicator).

Unlike the approach I had based my initial work on, there is no need for a command ID when changing LED state. Instead one simply specifies a board type and the LED number to change. It is possible this was changed in the Virpil firmware during the last few years, but that's guessing on my part.

Consider the following packets as examples:

**Set LED 11 on the on-board controller to full red:**

`02, 66, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 83, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, F0`

**Set LED 1 on the on-board controller to full green:**

`02, 66, 00, 00, 00, 8C, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, F0`

Ignoring the starting and ending bytes, the only packet data that is relevant to the LED change is the second byte (board type) along with the color byte in the position corresponding to the LED number on the board. The latter starts at the 6th byte in the packet. 

Additionally, there are a few reserved board types which can be used to set all LEDs on a board to a specific color or to reset the LEDs to their firmware defaults. Again the approach is pretty simple:

To set all LEDs on a board to the same color (note that the desired color is specified in the 6th byte of the packet):

`02, 65, 00, 00, 00, A0, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, F0`

It is possible to reset the board to firmware specified color defaults. In this case the color byte in position 6 is ignored and the board will reset to its default state:

`02, 64, 00, 00, 00, A0, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, F0`
