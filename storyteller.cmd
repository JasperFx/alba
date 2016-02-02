paket restore
C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" src/Alba.sln   /property:Configuration=debug /v:m /t:rebuild /nr:False /maxcpucount:2
packages\Storyteller\tools\st.exe open src\Alba.Testing
