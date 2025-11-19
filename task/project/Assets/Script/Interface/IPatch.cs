using System;
using System.Collections;

public interface IPatch
{
    int GetPatchCount();
    IEnumerator Patch(Action<int> onProgress);
}
