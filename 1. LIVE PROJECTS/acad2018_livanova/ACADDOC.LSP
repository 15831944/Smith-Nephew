(setvar "cmdecho" 0)
(princ "\nLoading AutoCAD 2018 LivaNova Interface...")
(command "._NETLOAD" "C:/ACAD2018_LivaNova/Interface/SSDL.dll")
(LOAD "C:/ACAD2018_LivaNova/LOAD-MSACCESS-BOMDATA.LSP")
(LOAD "C:/ACAD2018_LivaNova/LOAD-A2K-SUPPORT-SITE.LSP")
(LOAD "C:/ACAD2018_LivaNova/LOAD-VIDEO-FOLDER.LSP")
(LOAD "C:/ACAD2018_LivaNova/LOAD-IMPORT.LSP")
(LOAD "C:/ACAD2018_LivaNova/FILTER.LSP")
(LOAD "C:/ACAD2018_LivaNova/LOAD-FILE-EXPLORER.LSP")
(princ "Loading Complete...\n\n")
(setvar "cmdecho" 1)