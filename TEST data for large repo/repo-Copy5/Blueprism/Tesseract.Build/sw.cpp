void build(Solution &s)
{
    auto &tess = s.addProject("google.tesseract", "4.0.0");
    tess += Git("https://github.com/tesseract-ocr/tesseract", "", "{v}");

    auto &libtesseract = tess.addTarget<LibraryTarget>("libtesseract");
    {
        libtesseract.setChecks("libtesseract");

        libtesseract.ExportAllSymbols = true;
        libtesseract.PackageDefinitions = true;
        libtesseract +=
            "src/api/.*\\.cpp"_rr,
            "src/api/.*\\.h"_rr,
            "src/api/tess_version.h.in",
            "src/arch/.*\\.cpp"_rr,
            "src/arch/.*\\.h"_rr,
            "src/ccmain/.*\\.cpp"_rr,
            "src/ccmain/.*\\.h"_rr,
            "src/ccstruct/.*\\.cpp"_rr,
            "src/ccstruct/.*\\.h"_rr,
            "src/ccutil/.*\\.cpp"_rr,
            "src/ccutil/.*\\.h"_rr,
            "src/classify/.*\\.cpp"_rr,
            "src/classify/.*\\.h"_rr,
            "src/cutil/.*\\.cpp"_rr,
            "src/cutil/.*\\.h"_rr,
            "src/dict/.*\\.cpp"_rr,
            "src/dict/.*\\.h"_rr,
            "src/lstm/.*\\.cpp"_rr,
            "src/lstm/.*\\.h"_rr,
            "src/opencl/.*\\.cpp"_rr,
            "src/opencl/.*\\.h"_rr,
            "src/textord/.*\\.cpp"_rr,
            "src/textord/.*\\.h"_rr,
            "src/viewer/.*\\.cpp"_rr,
            "src/viewer/.*\\.h"_rr,
            "src/vs2010/port/.*"_rr,
            "src/wordrec/.*\\.cpp"_rr,
            "src/wordrec/.*\\.h"_rr;

        libtesseract -=
            "src/api/tesseractmain.cpp",
            "src/viewer/svpaint.cpp";

        libtesseract.Public +=
            "src/vs2010/port"_id,
            "src/opencl"_id,
            "src/ccmain"_id,
            "src/api"_id,
            "src/dict"_id,
            "src/viewer"_id,
            "src/wordrec"_id,
            "src/ccstruct"_id,
            "src/cutil"_id,
            "src/textord"_id,
            "src/ccutil"_id,
            "src/lstm"_id,
            "src/classify"_id,
            "src/arch"_id;

        if (s.Settings.Native.CompilerType == CompilerType::MSVC ||
            s.Settings.Native.CompilerType == CompilerType::ClangCl)
        {
            libtesseract["src/arch/dotproductavx.cpp"].args.push_back("-arch:AVX");
            libtesseract["src/arch/dotproductsse.cpp"].args.push_back("-D__SSE4_1__");
            libtesseract["src/arch/intsimdmatrixavx2.cpp"].args.push_back("-arch:AVX2");
            libtesseract["src/arch/intsimdmatrixsse.cpp"].args.push_back("-D__SSE4_1__");
        }

        libtesseract.Public += "HAVE_CONFIG_H"_d;
        libtesseract.Public += "WINDLLNAME=\"tesseract\""_d;
        libtesseract.Public += "_SILENCE_STDEXT_HASH_DEPRECATION_WARNINGS=1"_d;
        libtesseract.Interface += sw::Shared, "TESS_IMPORTS"_d;
        libtesseract.Private += sw::Shared, "TESS_EXPORTS"_d;

        libtesseract.Public += "org.sw.demo.danbloomberg.leptonica-master"_dep;

        if (s.Settings.TargetOS.Type == OSType::Windows)
            libtesseract.Public += "ws2_32.lib"_l;

        libtesseract.Variables["TESSERACT_MAJOR_VERSION"] = libtesseract.Variables["PACKAGE_MAJOR_VERSION"];
        libtesseract.Variables["TESSERACT_MINOR_VERSION"] = libtesseract.Variables["PACKAGE_MINOR_VERSION"];
        libtesseract.Variables["TESSERACT_MICRO_VERSION"] = libtesseract.Variables["PACKAGE_PATCH_VERSION"];
        libtesseract.Variables["TESSERACT_VERSION_STR"] = "master";
        libtesseract.configureFile("src/api/tess_version.h.in", "tess_version.h");
    }

    //
    auto &tesseract = tess.addExecutable("tesseract");
    tesseract += "src/api/tesseractmain.cpp";
    tesseract += libtesseract;

}

void check(Checker &c)
{
    auto &s = c.addSet("libtesseract");
    s.checkFunctionExists("getline");
    s.checkIncludeExists("dlfcn.h");
    s.checkIncludeExists("inttypes.h");
    s.checkIncludeExists("limits.h");
    s.checkIncludeExists("malloc.h");
    s.checkIncludeExists("memory.h");
    s.checkIncludeExists("stdbool.h");
    s.checkIncludeExists("stdint.h");
    s.checkIncludeExists("stdlib.h");
    s.checkIncludeExists("string.h");
    s.checkIncludeExists("sys/ipc.h");
    s.checkIncludeExists("sys/shm.h");
    s.checkIncludeExists("sys/stat.h");
    s.checkIncludeExists("sys/types.h");
    s.checkIncludeExists("sys/wait.h");
    s.checkIncludeExists("tiffio.h");
    s.checkIncludeExists("unistd.h");
    s.checkTypeSize("long long int");
    s.checkTypeSize("mbstate_t");
    s.checkTypeSize("off_t");
    s.checkTypeSize("size_t");
    s.checkTypeSize("void *");
    s.checkTypeSize("wchar_t");
    s.checkTypeSize("_Bool");
    {
        auto &c = s.checkSymbolExists("snprintf");
        c.Parameters.Includes.push_back("stdio.h");
    }
}
