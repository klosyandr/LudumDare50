﻿using System;
using System.Collections.Generic;

[Serializable]
public class StringListWrapper
{
    public List<string> list = new List<string>();

    public override string ToString()
    {
        return string.Join(",", list);
    }
}
