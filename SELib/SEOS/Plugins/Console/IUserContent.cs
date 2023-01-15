using System;
using System.Collections.Generic;

namespace IngameScript
{
    interface IUserContent
    {
        IEnumerable<ContentPage> OnBuild();
    }
}