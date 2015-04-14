// Copyright (C) 2005-2012 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "stdafx.h"
#include "callback.h"
#include "mvrInterfaces.h"

using namespace std;

class MPMadPresenter : public CUnknown, public IOsdRenderCallback, public CCritSec
{
  public:
    MPMadPresenter(IVMR9Callback* pCallback, IDirect3DDevice9* pDevice);
    ~MPMadPresenter();

    IBaseFilter* Initialize();

    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
    STDMETHODIMP QueryInterface(REFIID riid, void** ppvObject);

    ULONG STDMETHODCALLTYPE AddRef();
    ULONG STDMETHODCALLTYPE Release();
    ULONG STDMETHODCALLTYPE NonDelegatingAddRef();
    ULONG STDMETHODCALLTYPE NonDelegatingRelease();

    STDMETHODIMP ClearBackground(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP RenderOsd(LPCSTR name, REFERENCE_TIME frameStart, RECT *fullOutputRect, RECT *activeVideoRect);
    STDMETHODIMP SetDevice(IDirect3DDevice9* pD3DDev);

  private:
    HRESULT RenderToTexture(IDirect3DTexture9* pTexture, IDirect3DSurface9* pSurface);
    HRESULT RenderTexture(IDirect3DVertexBuffer9* pVertexBuf, IDirect3DTexture9* pTexture);

    HRESULT SetupOSDVertex(IDirect3DVertexBuffer9* pVertextBuf);
    HRESULT StoreMadDeviceState();
    HRESULT SetupMadDeviceState();
    HRESULT RestoreMadDeviceState();

    IDirect3DDevice9Ex* m_pDevice = nullptr;
    IDirect3DDevice9Ex* m_pMadD3DDev = nullptr;

    IVMR9Callback* m_pCallback = nullptr;
    
    IUnknown* m_pMad = nullptr;

    IDirect3DSurface9* m_pRenderSurfaceGui = nullptr;
    IDirect3DTexture9* m_pRenderTextureGui = nullptr;

    IDirect3DSurface9* m_pRenderSurfaceOsd = nullptr;
    IDirect3DTexture9* m_pRenderTextureOsd = nullptr;

    IDirect3DSurface9* m_pMPSurfaceGui = nullptr;
    IDirect3DTexture9* m_pMPTextureGui = nullptr;

    IDirect3DSurface9* m_pMPSurfaceOsd = nullptr;
    IDirect3DTexture9* m_pMPTextureOsd = nullptr;

    IDirect3DVertexBuffer9* m_pMadGuiVertexBuffer = nullptr;
    IDirect3DVertexBuffer9* m_pMadOsdVertexBuffer = nullptr;

    HANDLE m_hSharedGuiHandle = nullptr;
    HANDLE m_hSharedOsdHandle = nullptr;

    // stored mad device state
    IDirect3DVertexShader9* m_pOldVS = nullptr;
    IDirect3DVertexBuffer9* m_pOldStreamData = nullptr;
    IDirect3DBaseTexture9* m_pOldTexture = nullptr;

    DWORD m_dwOldFVF = 0;
    DWORD m_dwOldALPHABLENDENABLE = 0;
    DWORD m_dwOldSRCALPHA = 0;
    DWORD m_dwOldINVSRCALPHA = 0;
    UINT  m_nOldOffsetInBytes = 0;
    UINT  m_nOldStride = 0;
    RECT  m_oldScissorRect;
};

