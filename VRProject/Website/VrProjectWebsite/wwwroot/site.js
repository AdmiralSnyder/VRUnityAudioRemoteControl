const readUploadedFileAsText = (inputFile) => {
    const temporaryFileReader = new FileReader();
    return new Promise((resolve, reject) => {
        temporaryFileReader.onerror = () => {
            temporaryFileReader.abort();
            reject(new DOMException("Problem parsing input file."));
        };
        temporaryFileReader.addEventListener("load", function () {
            resolve(temporaryFileReader.result.split(',')[1]);
        }, false);
        temporaryFileReader.readAsDataURL(inputFile.files[0]);
    });
};

const getUploadedFileName = (inputFile) => {
    return new Promise((resolve) => {
        setTimeout(() => {
            resolve(inputFile.value);
        }, 20);
    });
};

getFileData = function (inputFile) {
    return readUploadedFileAsText(inputFile);
};

getFileName = function (inputFile) {
    return getUploadedFileName(inputFile);
};
//Blazor.registerFunction("getFileData", function (inputFile) {

//    return readUploadedFileAsText(inputFile);
//});