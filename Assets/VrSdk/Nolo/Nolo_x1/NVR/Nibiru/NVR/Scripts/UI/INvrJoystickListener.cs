// Copyright 2016 Nibiru. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// This script provides an interface for VR Joystick
///  
public interface INibiruJoystickListener
{
    void OnPressL1();

    void OnPressL2();

    void OnPressR1();

    void OnPressR2();

    void OnPressX();

    void OnPressY();

    void OnPressA();

    void OnPressB();

    void OnPressSelect();

    void OnPressStart();

    void OnPressDpadUp();

    void OnPressDpadDown();

    void OnPressDpadLeft();

    void OnPressDpadRight();

    // 左摇杆
    void OnLeftStickX(float axisValue);

    void OnLeftStickY(float axisValue);

    void OnLeftStickDown();

    // 右摇杆
    void OnRightStickX(float axisValue);

    void OnRightStickY(float axisValue);

    void OnRightStickDown();
}