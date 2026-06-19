# Virpil LED Control Script for SPAD.neXt

## Purpose
This script fills the functionality gap in **SPAD.neXt** regarding the direct control of LEDs on compatible **Virpil** devices.
It allows SPAD.neXt to manipulate button lights on specific Virpil hardware rather than relying on official configuration software.
As a result it allows dynamic lighting control integrated directly into your SPAD.neXt workflows and automations.

## Highlights
- **Solid Color Control**: Set fixed colors on buttons via the SPAD.neXt application interface.
- **Automated Cycles**: Configure color cycling with user-defined intervals.
- **Flexible Integration**: Functions as a standard external script, triggerable from any SPAD.neXt state change or rule.
- **Instrumented via JSON**: Configuration is handled through a simple JSON structure passed to the script as an argument.

## Installation:
1. **Place in SPAD.neXt Addons directory**: Unpack the DLL files into the SPAD.neXt Addons directory, normally located in %userprofile%\Documents\SPAD.neXt\Addons. Create the Addons directory if it does not exist. SPAD.neXt will automatically load the assembly on startup and initialize the script. See the official documentation at https://docs.spadnext.com/extending-and-apis/scripting-interface/c-scripting-precompile for information.
2. **Configure SPAD.neXt**: When creating a new rule or state change, you can trigger the script by specifying "External Script" as an action and picking "VirpilLightAutomationScript" from the dropdown. Paste the Json snippet defined in the Usage section below into the provided argument text box.

![img.png](blob:https://github.com/a6d2ac44-763b-49f1-9fb6-abcaac168554)

## Usage
1.  **Get Device IDs**: Open the VPC Configuration Tool, go to your device view, and note the Vendor ID (VID) and Product ID (PID). Use these values exactly as displayed (check if they are hex or decimal).
2.  **Identify the Button**: Find the button number corresponding to your target LED in the VPC tool's monitoring tab.
3.  **Build Your Configuration**: Create a single-line JSON object using the template below:
```
{"Vid":3344,"Pid":4259,"Button":3,"Colors":[{"R":"Off","G":"Full","B":"Off"},{"R":"Full","G":"Off","B":"Off"}],"IntervalMs":500}
```
Note: Although formatted on one line, the structure is: DeviceID→Button→ColorArray→Interval. Copy-paste exactly as shown.

**JSON Field Reference:**

| Field | Description | Required? |
|:-------|:---------------------------------|:----------|
| `Vid` | Device Vendor ID (from VPC tool) | ✅ Yes |
| `Pid` | Device Product ID (from VPC tool) | ✅ Yes |
| `Button` | Target button/LED number | ✅ Yes |
| `Colors` | Array of RGB states (`"Off"`, `"Thirty"`, `"Sixty"`, `"Full"`) | ✅ Yes |
| `IntervalMs` | Milliseconds between color changes when cycling | ❌ Optional (required if using >1 color) |

4.  **Apply in SPAD.neXt**: Create a new rule → Add Action → Select `External Script` → Choose `VirpilLightAutomationScript` → Paste your JSON string into the argument box.
5.  **Troubleshooting**: If lights don't respond, check `%appdata%\SPAD.neXt\logs`. The script logs all configuration payloads and HID errors there.

If you are encountering errors, remember to check the log files at %appdata%\SPAD.neXt\logs. The script will write any messages to the standard application log.

## Attribution
This project was inspired by https://github.com/charliefoxtwo/Virpil-Communicator and implements logic adapted from that library to in order to create the data structure
which controls the Virpil LED lights. As a result, this script is released under the same GNU v3.0 license.

## Limitations
The script is designed to work with Virpil LED lights and may not work with other types of lights. Additionally, the script may not work with all versions of SPAD.neXt. The script is provided as-is and the author is not responsible for any damages or issues that may arise from using the script.