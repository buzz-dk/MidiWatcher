using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiWatcher.Models;
public class ControlMessage
{
    public int Control
    {
        get; set;
    }
    public int Value
    {
        get; set;
    }
}
