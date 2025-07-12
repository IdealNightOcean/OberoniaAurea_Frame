using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OberoniaAurea_Frame;

public interface IFixedCaravanAssociate
{
    void Notify_FixedCaravanLeaveByPlayer(FixedCaravan fixedCaravan);
    string FixedCaravanWorkDesc();
}
