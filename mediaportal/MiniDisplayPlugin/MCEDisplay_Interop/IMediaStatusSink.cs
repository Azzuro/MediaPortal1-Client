﻿namespace MCEDisplay_Interop
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("075FC453-F236-41DA-B90D-9FBB8BBDC101"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaStatusSink
    {
        [DispId(1)]
        void Initialize();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(2)]
        IMediaStatusSession CreateSession();
    }
}

