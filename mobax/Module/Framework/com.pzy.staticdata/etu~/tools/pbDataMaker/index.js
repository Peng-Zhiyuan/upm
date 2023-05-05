const protobuf = require("protobufjs");
const zlib = require("zlib")
const fs = require("fs")
const path = require("path")

//let workPath = process.argv[2]

// json 数据文件
let jsonPath = process.argv[2]

// proto 定义文件
let protoPath = process.argv[3]

// json 数据对象对应的 proto 类型定义，需要带 pakcage。 如(Pb.StaticData)
let protoType = process.argv[4]

// 输出数据文件地址
let outFilePath = process.argv[5]

// if(!workPath) {
//     console.log( "work path empty" )
//     return
// }

const json = require(jsonPath);

protobuf.load(protoPath, function(err, root) {
    if (err) {
        throw err;
    }

    let Message = root.lookupType(protoType);
    //let newJson = {}
    let payload = json//{dic:json}//{ awesomeField: "AwesomeString" };

    let errMsg = Message.verify(payload);
    if (errMsg){
        throw Error(errMsg);
    }
    let message = Message.create(payload); // or use .fromObject if conversion is necessary
    let buffer = Message.encode(message).finish();

    fs.writeFileSync(outFilePath, buffer)
    console.log(`writing StaticData.dat.......done!`)

    // let rs = fs.createReadStream(path.resolve(workPath,  "StaticData.dat"));
    // let gzip = zlib.createGzip();
    // let ws = fs.createWriteStream(path.resolve(workPath,  "StaticData.zipeddat"))
    // rs.pipe(gzip).pipe(ws)
    // console.log(`zip StaticData.dat to StaticData.zipeddat.......done!`)

});