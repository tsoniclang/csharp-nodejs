import assert from "node:assert/strict";
import test from "node:test";
import { getCsharpNullableElementTargetType } from "../../../../tsonic-csharp/dist/public/provider.js";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

const relationsByJsSelection = new Map([
  [false, nodejsProviderTargetRelations(false)],
  [true, nodejsProviderTargetRelations(true)],
]);

const auditedNativeCells = [
  [1,"node:process","CpuUsage.system",null,"type","int64"],
  [2,"node:process","CpuUsage.user",null,"type","int64"],
  [3,"node:fs/promises","truncate","node:fs/promises.truncate(System.String,System.Int64)","param len","int64"],
  [4,"node:fs","readSync","node:fs.readSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param position","int64"],
  [5,"node:fs","truncateSync","node:fs.truncateSync(System.String,System.Int64)","param len","int64"],
  [6,"node:fs","writeSync","node:fs.writeSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param position","int64"],
  [7,"node:fs","writeSync","node:fs.writeSync(System.Int32,System.String,System.Nullable`1,System.String)","param position","int64"],
  [8,"node:process","MemoryUsage.arrayBuffers",null,"type","int64"],
  [9,"node:process","MemoryUsage.external",null,"type","int64"],
  [10,"node:process","MemoryUsage.heapTotal",null,"type","int64"],
  [11,"node:process","MemoryUsage.heapUsed",null,"type","int64"],
  [12,"node:process","MemoryUsage.rss",null,"type","int64"],
  [13,"node:os","freemem","node:os.freemem()","return","int64"],
  [14,"node:os","totalmem","node:os.totalmem()","return","int64"],
  [15,"node:os","uptime","node:os.uptime()","return","int64"],
  [16,"node:process","availableMemory","node:process.availableMemory()","return","int64"],
  [17,"node:process","constrainedMemory","node:process.constrainedMemory()","return","int64"],
  [18,"node:process","hrtime","node:process.hrtime(System.Int64[])","return[]","int64"],
  [19,"node:process","hrtime","node:process.hrtime(System.Int64[])","param time[]","int64"],
  [20,"node:fs","ReadStream.bytesRead",null,"type","int64"],
  [21,"node:fs","ReadStreamOptions.end",null,"type","int64"],
  [22,"node:fs","ReadStreamOptions.start",null,"type","int64"],
  [23,"node:net","Socket.bytesRead",null,"type","int64"],
  [24,"node:net","Socket.bytesWritten",null,"type","int64"],
  [25,"node:fs","Stats.size",null,"type","int64"],
  [26,"node:fs","WriteStream.bytesWritten",null,"type","int64"],
  [27,"node:fs","WriteStreamOptions.start",null,"type","int64"],
  [28,"node:buffer","Buffer.readUInt32BE","node:buffer.Buffer.readUInt32BE(System.Int32)","return","uint32"],
  [29,"node:buffer","Buffer.readUInt32LE","node:buffer.Buffer.readUInt32LE(System.Int32)","return","uint32"],
  [30,"node:buffer","Buffer.writeUInt32BE","node:buffer.Buffer.writeUInt32BE(System.UInt32,System.Int32)","param value","uint32"],
  [31,"node:buffer","Buffer.writeUInt32LE","node:buffer.Buffer.writeUInt32LE(System.UInt32,System.Int32)","param value","uint32"],
  [32,"node:fs","RmOptions.maxRetries",null,"type","uint32"],
  [33,"node:buffer","Buffer.readUInt16BE","node:buffer.Buffer.readUInt16BE(System.Int32)","return","uint16"],
  [34,"node:buffer","Buffer.readUInt16LE","node:buffer.Buffer.readUInt16LE(System.Int32)","return","uint16"],
  [35,"node:buffer","Buffer.writeUInt16BE","node:buffer.Buffer.writeUInt16BE(System.UInt16,System.Int32)","param value","uint16"],
  [36,"node:buffer","Buffer.writeUInt16LE","node:buffer.Buffer.writeUInt16LE(System.UInt16,System.Int32)","param value","uint16"],
  [37,"node:buffer","Buffer.readInt16BE","node:buffer.Buffer.readInt16BE(System.Int32)","return","int16"],
  [38,"node:buffer","Buffer.readInt16LE","node:buffer.Buffer.readInt16LE(System.Int32)","return","int16"],
  [39,"node:buffer","Buffer.writeInt16BE","node:buffer.Buffer.writeInt16BE(System.Int16,System.Int32)","param value","int16"],
  [40,"node:buffer","Buffer.writeInt16LE","node:buffer.Buffer.writeInt16LE(System.Int16,System.Int32)","param value","int16"],
  [41,"node:buffer","Buffer.readUInt8","node:buffer.Buffer.readUInt8(System.Int32)","return","uint8"],
  [42,"node:buffer","Buffer.writeUInt8","node:buffer.Buffer.writeUInt8(System.Byte,System.Int32)","param value","uint8"],
  [43,"node:buffer","Buffer.readInt8","node:buffer.Buffer.readInt8(System.Int32)","return","int8"],
  [44,"node:buffer","Buffer.writeInt8","node:buffer.Buffer.writeInt8(System.SByte,System.Int32)","param value","int8"],
  [45,"node:buffer","Buffer.readFloatBE","node:buffer.Buffer.readFloatBE(System.Int32)","return","float32"],
  [46,"node:buffer","Buffer.readFloatLE","node:buffer.Buffer.readFloatLE(System.Int32)","return","float32"],
  [47,"node:buffer","Buffer.writeFloatBE","node:buffer.Buffer.writeFloatBE(System.Single,System.Int32)","param value","float32"],
  [48,"node:buffer","Buffer.writeFloatLE","node:buffer.Buffer.writeFloatLE(System.Single,System.Int32)","param value","float32"],
  [49,"node:http","AddressInfo.port",null,"type","int32"],
  [50,"node:zlib","BrotliOptions.chunkSize",null,"type","int32"],
  [51,"node:zlib","BrotliOptions.maxOutputLength",null,"type","int32"],
  [52,"node:zlib","BrotliOptions.quality",null,"type","int32"],
  [53,"node:buffer","Buffer.alloc (static)","node:buffer.Buffer.alloc(System.Int32)","param size","int32"],
  [54,"node:buffer","Buffer.allocUnsafe (static)","node:buffer.Buffer.allocUnsafe(System.Int32)","param size","int32"],
  [55,"node:buffer","Buffer.allocUnsafeSlow (static)","node:buffer.Buffer.allocUnsafeSlow(System.Int32)","param size","int32"],
  [56,"node:buffer","Buffer.byteLength (static)","node:buffer.Buffer.byteLength(System.String,System.String)","return","int32"],
  [57,"node:buffer","Buffer.compare (static)","node:buffer.Buffer.compare(Tsonic.CSharp.Node.Buffer,Tsonic.CSharp.Node.Buffer)","return","int32"],
  [58,"node:buffer","Buffer.compare","node:buffer.Buffer.prototype.compare(Tsonic.CSharp.Node.Buffer)","return","int32"],
  [59,"node:buffer","Buffer.concat (static)","node:buffer.Buffer.concat(System.Collections.Generic.IReadOnlyList`1<Tsonic.CSharp.Node.Buffer>,System.Nullable`1)","param totalLength","int32"],
  [60,"node:buffer","Buffer.copy","node:buffer.Buffer.copy(Tsonic.CSharp.Node.Buffer,System.Int32,System.Nullable`1,System.Nullable`1)","return","int32"],
  [61,"node:buffer","Buffer.copy","node:buffer.Buffer.copy(Tsonic.CSharp.Node.Buffer,System.Int32,System.Nullable`1,System.Nullable`1)","param targetStart","int32"],
  [62,"node:buffer","Buffer.copy","node:buffer.Buffer.copy(Tsonic.CSharp.Node.Buffer,System.Int32,System.Nullable`1,System.Nullable`1)","param sourceStart","int32"],
  [63,"node:buffer","Buffer.copy","node:buffer.Buffer.copy(Tsonic.CSharp.Node.Buffer,System.Int32,System.Nullable`1,System.Nullable`1)","param sourceEnd","int32"],
  [64,"node:buffer","Buffer.from (static)","node:buffer.Buffer.from(System.Int32[])","param array[]","int32"],
  [65,"node:buffer","Buffer.includes","node:buffer.Buffer.includes(System.Object,System.Int32,System.String)","param byteOffset","int32"],
  [66,"node:buffer","Buffer.indexOf","node:buffer.Buffer.indexOf(System.Object,System.Int32,System.String)","return","int32"],
  [67,"node:buffer","Buffer.indexOf","node:buffer.Buffer.indexOf(System.Object,System.Int32,System.String)","param byteOffset","int32"],
  [68,"node:buffer","Buffer.lastIndexOf","node:buffer.Buffer.lastIndexOf(System.Object,System.Nullable`1,System.String)","return","int32"],
  [69,"node:buffer","Buffer.lastIndexOf","node:buffer.Buffer.lastIndexOf(System.Object,System.Nullable`1,System.String)","param byteOffset","int32"],
  [70,"node:buffer","Buffer.length",null,"type","int32"],
  [71,"node:buffer","Buffer.poolSize (static)",null,"type","int32"],
  [72,"node:buffer","Buffer.readDoubleBE","node:buffer.Buffer.readDoubleBE(System.Int32)","param offset","int32"],
  [73,"node:buffer","Buffer.readDoubleLE","node:buffer.Buffer.readDoubleLE(System.Int32)","param offset","int32"],
  [74,"node:buffer","Buffer.readFloatBE","node:buffer.Buffer.readFloatBE(System.Int32)","param offset","int32"],
  [75,"node:buffer","Buffer.readFloatLE","node:buffer.Buffer.readFloatLE(System.Int32)","param offset","int32"],
  [76,"node:buffer","Buffer.readInt16BE","node:buffer.Buffer.readInt16BE(System.Int32)","param offset","int32"],
  [77,"node:buffer","Buffer.readInt16LE","node:buffer.Buffer.readInt16LE(System.Int32)","param offset","int32"],
  [78,"node:buffer","Buffer.readInt32BE","node:buffer.Buffer.readInt32BE(System.Int32)","return","int32"],
  [79,"node:buffer","Buffer.readInt32BE","node:buffer.Buffer.readInt32BE(System.Int32)","param offset","int32"],
  [80,"node:buffer","Buffer.readInt32LE","node:buffer.Buffer.readInt32LE(System.Int32)","return","int32"],
  [81,"node:buffer","Buffer.readInt32LE","node:buffer.Buffer.readInt32LE(System.Int32)","param offset","int32"],
  [82,"node:buffer","Buffer.readInt8","node:buffer.Buffer.readInt8(System.Int32)","param offset","int32"],
  [83,"node:buffer","Buffer.readUInt16BE","node:buffer.Buffer.readUInt16BE(System.Int32)","param offset","int32"],
  [84,"node:buffer","Buffer.readUInt16LE","node:buffer.Buffer.readUInt16LE(System.Int32)","param offset","int32"],
  [85,"node:buffer","Buffer.readUInt32BE","node:buffer.Buffer.readUInt32BE(System.Int32)","param offset","int32"],
  [86,"node:buffer","Buffer.readUInt32LE","node:buffer.Buffer.readUInt32LE(System.Int32)","param offset","int32"],
  [87,"node:buffer","Buffer.readUInt8","node:buffer.Buffer.readUInt8(System.Int32)","param offset","int32"],
  [88,"node:buffer","Buffer.slice","node:buffer.Buffer.slice(System.Nullable`1,System.Nullable`1)","param start","int32"],
  [89,"node:buffer","Buffer.slice","node:buffer.Buffer.slice(System.Nullable`1,System.Nullable`1)","param end","int32"],
  [90,"node:buffer","Buffer.subarray","node:buffer.Buffer.subarray(System.Nullable`1,System.Nullable`1)","param start","int32"],
  [91,"node:buffer","Buffer.subarray","node:buffer.Buffer.subarray(System.Nullable`1,System.Nullable`1)","param end","int32"],
  [92,"node:buffer","Buffer.toString","node:buffer.Buffer.toString(System.String,System.Int32,System.Nullable`1)","param start","int32"],
  [93,"node:buffer","Buffer.toString","node:buffer.Buffer.toString(System.String,System.Int32,System.Nullable`1)","param end","int32"],
  [94,"node:buffer","Buffer.write","node:buffer.Buffer.write(System.String,System.Int32,System.Nullable`1,System.String)","return","int32"],
  [95,"node:buffer","Buffer.write","node:buffer.Buffer.write(System.String,System.Int32,System.Nullable`1,System.String)","param offset","int32"],
  [96,"node:buffer","Buffer.write","node:buffer.Buffer.write(System.String,System.Int32,System.Nullable`1,System.String)","param length","int32"],
  [97,"node:buffer","Buffer.writeDoubleBE","node:buffer.Buffer.writeDoubleBE(System.Double,System.Int32)","return","int32"],
  [98,"node:buffer","Buffer.writeDoubleBE","node:buffer.Buffer.writeDoubleBE(System.Double,System.Int32)","param offset","int32"],
  [99,"node:buffer","Buffer.writeDoubleLE","node:buffer.Buffer.writeDoubleLE(System.Double,System.Int32)","return","int32"],
  [100,"node:buffer","Buffer.writeDoubleLE","node:buffer.Buffer.writeDoubleLE(System.Double,System.Int32)","param offset","int32"],
  [101,"node:buffer","Buffer.writeFloatBE","node:buffer.Buffer.writeFloatBE(System.Single,System.Int32)","return","int32"],
  [102,"node:buffer","Buffer.writeFloatBE","node:buffer.Buffer.writeFloatBE(System.Single,System.Int32)","param offset","int32"],
  [103,"node:buffer","Buffer.writeFloatLE","node:buffer.Buffer.writeFloatLE(System.Single,System.Int32)","return","int32"],
  [104,"node:buffer","Buffer.writeFloatLE","node:buffer.Buffer.writeFloatLE(System.Single,System.Int32)","param offset","int32"],
  [105,"node:buffer","Buffer.writeInt16BE","node:buffer.Buffer.writeInt16BE(System.Int16,System.Int32)","return","int32"],
  [106,"node:buffer","Buffer.writeInt16BE","node:buffer.Buffer.writeInt16BE(System.Int16,System.Int32)","param offset","int32"],
  [107,"node:buffer","Buffer.writeInt16LE","node:buffer.Buffer.writeInt16LE(System.Int16,System.Int32)","return","int32"],
  [108,"node:buffer","Buffer.writeInt16LE","node:buffer.Buffer.writeInt16LE(System.Int16,System.Int32)","param offset","int32"],
  [109,"node:buffer","Buffer.writeInt32BE","node:buffer.Buffer.writeInt32BE(System.Int32,System.Int32)","return","int32"],
  [110,"node:buffer","Buffer.writeInt32BE","node:buffer.Buffer.writeInt32BE(System.Int32,System.Int32)","param value","int32"],
  [111,"node:buffer","Buffer.writeInt32BE","node:buffer.Buffer.writeInt32BE(System.Int32,System.Int32)","param offset","int32"],
  [112,"node:buffer","Buffer.writeInt32LE","node:buffer.Buffer.writeInt32LE(System.Int32,System.Int32)","return","int32"],
  [113,"node:buffer","Buffer.writeInt32LE","node:buffer.Buffer.writeInt32LE(System.Int32,System.Int32)","param value","int32"],
  [114,"node:buffer","Buffer.writeInt32LE","node:buffer.Buffer.writeInt32LE(System.Int32,System.Int32)","param offset","int32"],
  [115,"node:buffer","Buffer.writeInt8","node:buffer.Buffer.writeInt8(System.SByte,System.Int32)","return","int32"],
  [116,"node:buffer","Buffer.writeInt8","node:buffer.Buffer.writeInt8(System.SByte,System.Int32)","param offset","int32"],
  [117,"node:buffer","Buffer.writeUInt16BE","node:buffer.Buffer.writeUInt16BE(System.UInt16,System.Int32)","return","int32"],
  [118,"node:buffer","Buffer.writeUInt16BE","node:buffer.Buffer.writeUInt16BE(System.UInt16,System.Int32)","param offset","int32"],
  [119,"node:buffer","Buffer.writeUInt16LE","node:buffer.Buffer.writeUInt16LE(System.UInt16,System.Int32)","return","int32"],
  [120,"node:buffer","Buffer.writeUInt16LE","node:buffer.Buffer.writeUInt16LE(System.UInt16,System.Int32)","param offset","int32"],
  [121,"node:buffer","Buffer.writeUInt32BE","node:buffer.Buffer.writeUInt32BE(System.UInt32,System.Int32)","return","int32"],
  [122,"node:buffer","Buffer.writeUInt32BE","node:buffer.Buffer.writeUInt32BE(System.UInt32,System.Int32)","param offset","int32"],
  [123,"node:buffer","Buffer.writeUInt32LE","node:buffer.Buffer.writeUInt32LE(System.UInt32,System.Int32)","return","int32"],
  [124,"node:buffer","Buffer.writeUInt32LE","node:buffer.Buffer.writeUInt32LE(System.UInt32,System.Int32)","param offset","int32"],
  [125,"node:buffer","Buffer.writeUInt8","node:buffer.Buffer.writeUInt8(System.Byte,System.Int32)","return","int32"],
  [126,"node:buffer","Buffer.writeUInt8","node:buffer.Buffer.writeUInt8(System.Byte,System.Int32)","param offset","int32"],
  [127,"node:tls","ConnectionOptions.port",null,"type","int32"],
  [128,"node:tls","ConnectionOptions.timeout",null,"type","int32"],
  [129,"node:net","ConnectOptions.port",null,"type","int32"],
  [130,"node:crypto","randomBytes","node:crypto.randomBytes(System.Int32)","param size","int32"],
  [131,"node:crypto","randomFillSync","node:crypto.randomFillSync(Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32)","param offset","int32"],
  [132,"node:crypto","randomFillSync","node:crypto.randomFillSync(Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32)","param size","int32"],
  [133,"node:crypto","randomInt","node:crypto.randomInt(System.Int32,System.Int32?)","return","int32"],
  [134,"node:crypto","randomInt","node:crypto.randomInt(System.Int32,System.Int32?)","param minOrMax","int32"],
  [135,"node:crypto","randomInt","node:crypto.randomInt(System.Int32,System.Int32?)","param max","int32"],
  [136,"node:dns","lookup","node:dns.lookup(hostname,callback)","param callback cb.family","int32"],
  [137,"node:dns","lookup","node:dns.lookup(hostname,lookup-options,callback)","param callback cb.family","int32"],
  [138,"node:events","EventEmitter.getMaxListeners","node:events.EventEmitter.getMaxListeners()","return","int32"],
  [139,"node:events","EventEmitter.listenerCount","node:events.EventEmitter.listenerCount(eventName)","return","int32"],
  [140,"node:events","EventEmitter.setMaxListeners","node:events.EventEmitter.setMaxListeners(count)","param count","int32"],
  [141,"node:fs/promises","access","node:fs/promises.access(System.String,System.Int32)","param mode","int32"],
  [142,"node:fs/promises","chmod","node:fs/promises.chmod(System.String,System.Int32)","param mode","int32"],
  [143,"node:fs/promises","copyFile","node:fs/promises.copyFile(System.String,System.String,System.Int32)","param mode","int32"],
  [144,"node:fs","accessSync","node:fs.accessSync(System.String,System.Int32)","param mode","int32"],
  [145,"node:fs","chmodSync","node:fs.chmodSync(System.String,System.Int32)","param mode","int32"],
  [146,"node:fs","closeSync","node:fs.closeSync(System.Int32)","param fd","int32"],
  [147,"node:fs","copyFileSync","node:fs.copyFileSync(System.String,System.String,System.Int32)","param mode","int32"],
  [148,"node:fs","fstatSync","node:fs.fstatSync(System.Int32)","param fd","int32"],
  [149,"node:fs","openSync","node:fs.openSync(System.String,System.String,System.Int32)","return","int32"],
  [150,"node:fs","openSync","node:fs.openSync(System.String,System.String,System.Int32)","param mode","int32"],
  [151,"node:fs","readSync","node:fs.readSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","return","int32"],
  [152,"node:fs","readSync","node:fs.readSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param fd","int32"],
  [153,"node:fs","readSync","node:fs.readSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param offset","int32"],
  [154,"node:fs","readSync","node:fs.readSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param length","int32"],
  [155,"node:fs","writeSync","node:fs.writeSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","return","int32"],
  [156,"node:fs","writeSync","node:fs.writeSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param fd","int32"],
  [157,"node:fs","writeSync","node:fs.writeSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param offset","int32"],
  [158,"node:fs","writeSync","node:fs.writeSync(System.Int32,Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32,System.Nullable`1)","param length","int32"],
  [159,"node:fs","writeSync","node:fs.writeSync(System.Int32,System.String,System.Nullable`1,System.String)","return","int32"],
  [160,"node:fs","writeSync","node:fs.writeSync(System.Int32,System.String,System.Nullable`1,System.String)","param fd","int32"],
  [161,"node:readline","Interface.cursor",null,"type","int32"],
  [162,"node:readline","ReadLineOptions.historySize",null,"type","int32"],
  [163,"node:net","ListenOptions.backlog",null,"type","int32"],
  [164,"node:net","ListenOptions.port",null,"type","int32"],
  [165,"node:dns","LookupAddress.family",null,"type","int32"],
  [166,"node:dns","LookupOptions.family",null,"type","int32"],
  [167,"node:dns","LookupOptions.hints",null,"type","int32"],
  [168,"node:fs","MakeDirectoryOptions.mode",null,"type","int32"],
  [169,"node:net","createConnection","node:net.createConnection(port,host,connectionListener)","param port","int32"],
  [170,"node:net","isIP","node:net.isIP(input)","return","int32"],
  [171,"node:os","availableParallelism","node:os.availableParallelism()","return","int32"],
  [172,"node:process","exit","node:process.exit(System.Nullable`1)","param code","int32"],
  [173,"node:process","pid",null,"type","int32"],
  [174,"node:process","ppid",null,"type","int32"],
  [175,"node:stream","Readable.read","node:stream.Readable.read(size)","param size","int32"],
  [176,"node:fs","ReadStreamOptions.highWaterMark",null,"type","int32"],
  [177,"node:http","RequestOptions.port",null,"type","int32"],
  [178,"node:fs","RmOptions.retryDelay",null,"type","int32"],
  [179,"node:net","Server.listen","node:net.Server.listen(port,hostname,callback)","param port","int32"],
  [180,"node:http","Server.listen","node:http.Server.listen(System.Int32,System.Action)","param port","int32"],
  [181,"node:http","Server.listen","node:http.Server.listen(System.Int32,System.String,System.Int32,System.Action)","param port","int32"],
  [182,"node:http","Server.listen","node:http.Server.listen(System.Int32,System.String,System.Action)","param port","int32"],
  [183,"node:http","ServerAddress.port",null,"type","int32"],
  [184,"node:http","ServerResponse.statusCode",null,"type","int32"],
  [185,"node:http","ServerResponse.writeHead","node:http.ServerResponse.writeHead(System.Int32)","param statusCode","int32"],
  [186,"node:http","ServerResponse.writeHead","node:http.ServerResponse.writeHead(System.Int32,OutgoingHttpHeaders)","param statusCode","int32"],
  [187,"node:http","ServerResponse.writeHead","node:http.ServerResponse.writeHead(System.Int32,System.String,OutgoingHttpHeaders)","param statusCode","int32"],
  [188,"node:net","Socket.connect","node:net.Socket.connect(port,host,connectionListener)","param port","int32"],
  [189,"node:child_process","SpawnSyncOptionsWithBufferEncoding.gid",null,"type","int32"],
  [190,"node:child_process","SpawnSyncOptionsWithBufferEncoding.maxBuffer",null,"type","int32"],
  [191,"node:child_process","SpawnSyncOptionsWithBufferEncoding.timeout",null,"type","int32"],
  [192,"node:child_process","SpawnSyncOptionsWithBufferEncoding.uid",null,"type","int32"],
  [193,"node:child_process","SpawnSyncReturns.pid",null,"type","int32"],
  [194,"node:timers","setInterval","node:timers.setInterval(Function,System.Int32)","param delay","int32"],
  [195,"node:timers","setTimeout","node:timers.setTimeout(Function,System.Int32)","param delay","int32"],
  [196,"node:tls","connect","node:tls.connect(port,host,options,secureConnectListener)","param port","int32"],
  [197,"node:tls","TlsOptions.handshakeTimeout",null,"type","int32"],
  [198,"node:url","URLSearchParams.size",null,"type","int32"],
  [199,"node:util","convertProcessSignalToExitCode","node:util.convertProcessSignalToExitCode(System.String)","return","int32"],
  [200,"node:util","getSystemErrorMessage","node:util.getSystemErrorMessage(System.Int32)","param err","int32"],
  [201,"node:util","getSystemErrorName","node:util.getSystemErrorName(System.Int32)","param err","int32"],
  [202,"node:worker_threads","threadId",null,"type","int32"],
  [203,"node:worker_threads","Worker.terminate","node:worker_threads.Worker.terminate()","return Promise","int32"],
  [204,"node:worker_threads","Worker.threadId",null,"type","int32"],
  [205,"node:fs","WriteStreamOptions.highWaterMark",null,"type","int32"],
  [206,"node:fs","WriteStreamOptions.mode",null,"type","int32"],
  [207,"node:zlib","ZlibOptions.chunkSize",null,"type","int32"],
  [208,"node:zlib","ZlibOptions.level",null,"type","int32"],
  [209,"node:zlib","ZlibOptions.maxOutputLength",null,"type","int32"],
  [210,"node:dns","LookupAllOptions.all",null,"type","true"],
  [211,"node:http","RequestOptions.timeout",null,"type","int32?"],
  [212,"node:http","Server.listen","node:http.Server.listen(System.Int32,System.String,System.Int32,System.Action)","param backlog","int32?"],
  [213,"node:net","Socket.localPort",null,"type","int32?"],
  [214,"node:net","Socket.remotePort",null,"type","int32?"],
];

for (const [id, moduleSpecifier, memberPath, signatureId, position, expectedKind] of auditedNativeCells) {
  test(`C2354 nodejs audit ${id}: ${moduleSpecifier} ${memberPath} ${position}`, () => {
    for (const includeJsSurfaceMembers of [false, true]) {
      const cell = publishedCell(moduleSpecifier, memberPath, signatureId, position, includeJsSurfaceMembers);
      const sourceType = presentSourceType(cell.sourceType);
      if (expectedKind === "true") {
        assert.deepEqual(sourceType, { kind: "literal", value: true });
        assert.equal(cell.optional, false, "the array-callback overload requires all:true");
        assert.equal(getCsharpNullableElementTargetType(cell.targetType)?.name, "bool");
      } else {
        assert.deepEqual(sourceType, {
          kind: "source-primitive",
          name: expectedKind.replace("?", ""),
        }, "the published Source primitive has the exact native width");
        const nullableTarget = getCsharpNullableElementTargetType(cell.targetType);
        const targetType = nullableTarget ?? cell.targetType;
        assert.equal(targetType.kind, "source-primitive");
        assert.equal(targetType.name, expectedKind.replace("?", ""));
        if (nullableTarget !== undefined) {
          assert.equal(
            cell.optional || hasSourceAbsence(cell.sourceType),
            true,
            "nullable native storage has published Source absence",
          );
        }
      }
    }
  });
}

test("C2354 one int64 filesystem position declaration replaces both former numeric routes", () => {
  const declarations = nodejsCanonicalProviderExports("node:fs");
  for (const [name, count] of [["readSync", 1], ["writeSync", 2]]) {
    const declaration = declarations.find(entry => entry.name === name && entry.exportKind !== "default");
    assert.equal(declaration?.signatures?.length, count);
    for (const signature of declaration.signatures) {
      assert.equal(signature.id.includes(".int64-position"), false);
      assert.deepEqual(presentSourceType(signature.parameters.find(parameter => parameter.name === "position").type), {
        kind: "source-primitive",
        name: "int64",
      });
    }
  }
});

test("C2354 real double members retain their floating contract", () => {
  const stats = nodejsCanonicalProviderExports("node:fs").find(entry => entry.name === "Stats");
  for (const name of ["atimeMs", "mtimeMs", "ctimeMs", "birthtimeMs"]) {
    assert.deepEqual(stats.members.find(member => member.name === name).type, { kind: "number" }, name);
  }
  const uptime = nodejsCanonicalProviderExports("node:process").find(entry =>
    entry.name === "uptime" && entry.exportKind !== "default");
  assert.deepEqual(uptime.signatures[0].returnType, { kind: "number" });
  const loadavg = nodejsCanonicalProviderExports("node:os").find(entry =>
    entry.name === "loadavg" && entry.exportKind !== "default");
  assert.deepEqual(loadavg.signatures[0].returnType, {
    kind: "array",
    elementType: { kind: "number" },
  });
});

test("C2355 DNS all:true remains the exact array-result overload discriminant", () => {
  const declarations = nodejsCanonicalProviderExports("node:dns");
  const options = declarations.find(entry => entry.name === "LookupAllOptions");
  const all = options.members.find(member => member.name === "all");
  assert.deepEqual(all.type, { kind: "literal", value: true });
  assert.notEqual(all.optional, true);
  const lookup = declarations.find(entry => entry.name === "lookup");
  const signature = lookup.signatures.find(entry => entry.id === "node:dns.lookup(hostname,lookup-all-options,callback)");
  assert.equal(signature.parameters[1].type.exportName, "LookupAllOptions");
  const callback = signature.parameters[2].type;
  assert.equal(callback.kind, "function");
  assert.equal(callback.parameters[1].type.kind, "array");
  assert.equal(callback.parameters[1].type.elementType.exportName, "LookupAddress");
});

test("nullable process and child-process status values retain their native int32 kind", () => {
  for (const [moduleSpecifier, memberPath] of [
    ["node:process", "exitCode"],
    ["node:child_process", "SpawnSyncReturns.status"],
  ]) {
    for (const includeJsSurfaceMembers of [false, true]) {
      const cell = publishedCell(moduleSpecifier, memberPath, null, "type", includeJsSurfaceMembers);
      assert.deepEqual(presentSourceType(cell.sourceType), { kind: "source-primitive", name: "int32" });
      assert.equal(hasSourceAbsence(cell.sourceType), true);
      assert.equal(getCsharpNullableElementTargetType(cell.targetType)?.name, "int32");
    }
  }
});

for (const surface of ["csharp", "js"]) {
  test(`C2354 native Node widths survive the actual ${surface} compiler composition`, () => {
    const compiled = compileCsharpSource({
      surface,
      capabilities: [createTsonicPlugin()],
      sourceText: `
        import { Buffer } from "node:buffer";
        import { readSync, statSync } from "node:fs";
        import process from "node:process";
        import { freemem } from "node:os";
        import type { float32, int32, int64, uint32 } from "@tsonic/core/types.js";

        export function fileSize(path: string): int64 {
          return statSync(path).size;
        }

        export function exactPosition(fd: int32, position: int64, bytes: Buffer): int32 {
          return readSync(fd, bytes, 0, bytes.length, position);
        }

        export function currentPosition(fd: int32, bytes: Buffer): int32 {
          return readSync(fd, bytes, 0, bytes.length);
        }

        export function memory(): int64 {
          return process.memoryUsage().rss + freemem();
        }

        export function halfCount(value: string, divisor: int32): int32 {
          return Buffer.byteLength(value) / divisor;
        }

        export function unsigned(value: uint32): uint32 {
          const bytes = Buffer.alloc(4);
          bytes.writeUInt32LE(value);
          return bytes.readUInt32LE();
        }

        export function single(value: float32): float32 {
          const bytes = Buffer.alloc(4);
          bytes.writeFloatLE(value);
          return bytes.readFloatLE();
        }
      `,
    });
    assertCsharpCompilationSucceeded(compiled);
    const source = compiled.artifacts.get("src/Index.cs");
    assert.match(source, /long fileSize\(string path\)/u);
    assert.match(source, /int exactPosition\(int fd, long position, Tsonic\.CSharp\.Node\.Buffer bytes\)/u);
    assert.match(source, /return Tsonic\.CSharp\.Node\.fs\.readSync\(fd, bytes, 0, bytes\.length, position\);/u);
    assert.match(source, /return Tsonic\.CSharp\.Node\.fs\.readSync\(fd, bytes, 0, bytes\.length\);/u);
    assert.match(source, /long memory\(\)/u);
    assert.match(source, /int halfCount\(string value, int divisor\)/u);
    assert.match(source, /Buffer\.byteLength\(value\) \/ divisor/u);
    assert.match(source, /uint unsigned\(uint value\)/u);
    assert.match(source, /float single\(float value\)/u);
    assert.doesNotMatch(source, /RequireInteger|System\.Convert\.|IntegerConversions|\(double\)/u);
  });
}

for (const surface of ["csharp", "js"]) {
  test(`C2380 process.kill uses native int32 inputs on ${surface}`, () => {
    const compiled = compileCsharpSource({
      surface,
      capabilities: [createTsonicPlugin()],
      sourceText: `
        import { kill } from "node:process";
        import type { int32 } from "@tsonic/core/types.js";

        export function omitted(pid: int32): boolean { return kill(pid); }
        export function absent(pid: int32): boolean { return kill(pid, undefined); }
        export function numeric(pid: int32, signal: int32): boolean { return kill(pid, signal); }
        export function named(pid: int32): boolean { return kill(pid, "SIGUSR1"); }
        export function exists(pid: int32): boolean { return kill(pid, 0); }
      `,
    });
    assertCsharpCompilationSucceeded(compiled);
    const source = compiled.artifacts.get("src/Index.cs");
    assert.match(source, /bool omitted\(int pid\)/u);
    assert.match(source, /bool numeric\(int pid, int signal\)/u);
    assert.match(source, /process\.killNumber\(pid, signal\)/u);
    assert.match(source, /process\.killNamed\(pid, "SIGUSR1"\)/u);
    assert.doesNotMatch(source, /kill(?:Named|Number)?<|IntegerConversions|processSignalInteger/u);
  });
}

function publishedCell(moduleSpecifier, memberPath, signatureId, position, includeJsSurfaceMembers) {
  const staticMember = memberPath.endsWith(" (static)");
  const [exportName, memberName] = memberPath.replace(/ \(static\)$/u, "").split(".");
  const declarations = nodejsCanonicalProviderExports(moduleSpecifier, includeJsSurfaceMembers);
  const declaration = declarations.find(entry => entry.name === exportName && entry.exportKind !== "default");
  assert.equal(declaration !== undefined, true, "the audited export exists");
  const members = memberName === undefined ? [declaration] : declaration.members?.filter(member =>
    member.name === memberName && (member.static === true) === staticMember &&
    (signatureId === null || member.signatures?.some(signature => signature.id === signatureId))) ?? [];
  assert.equal(members.length, 1, "one exact audited member matches staticness and signature");
  const selected = members[0];
  assert.equal(selected !== undefined, true, "the audited member exists");
  const signature = signatureId === null
    ? undefined
    : selected.signatures?.find(entry => entry.id === signatureId);
  assert.equal(signatureId === null || signature !== undefined, true, "the exact audited signature exists");
  const allRelations = relationsByJsSelection.get(includeJsSurfaceMembers);
  const relations = allRelations.filter(relation =>
    relation.source.providerModuleId === moduleSpecifier &&
    relation.source.exportId === declaration.id &&
    relation.source.memberId === (memberName === undefined ? undefined : selected.id) &&
    relation.source.signatureId === (signatureId ?? undefined) &&
    relation.kind !== "type");
  assert.deepEqual(relations.map(relation => relation.source.moduleSpecifier).sort(),
    [moduleSpecifier, moduleSpecifier.slice(5)].sort(), "canonical and bare imports select the same ABI");
  const relation = relations.find(entry => entry.source.moduleSpecifier === moduleSpecifier);
  let sourceType;
  let targetType;
  let optional = false;
  if (position === "type") {
    sourceType = selected.type;
    targetType = relation.targetMember.returnType;
    optional = selected.optional === true;
  } else if (position.startsWith("return")) {
    sourceType = signature.returnType;
    targetType = relation.targetMember.returnType;
  } else {
    const parameterName = position.slice("param ".length).split(" ")[0].replace("[]", "");
    const index = signature.parameters.findIndex(parameter => parameter.name === parameterName);
    assert.notEqual(index, -1, "the exact audited parameter exists");
    sourceType = signature.parameters[index].type;
    targetType = relation.targetMember.parameters[index].type;
    optional = signature.parameters[index].optional === true;
  }
  if (position.endsWith("[]")) {
    assert.equal(sourceType.kind, "array");
    assert.equal(targetType.kind, "array");
    sourceType = sourceType.elementType;
    targetType = targetType.element;
  }
  if (position.endsWith(" Promise")) {
    assert.equal(sourceType.kind, "source-global");
    assert.equal(sourceType.name, "Promise");
    sourceType = sourceType.typeArguments[0];
    targetType = targetType.typeArguments[0];
  }
  if (position.endsWith(" cb.family")) {
    assert.equal(sourceType.kind, "function");
    const index = sourceType.parameters.findIndex(parameter => parameter.name === "family");
    assert.notEqual(index, -1);
    sourceType = sourceType.parameters[index].type;
    targetType = targetType.typeArguments[index];
  }
  return { sourceType, targetType, optional };
}

function hasSourceAbsence(type) {
  return type.kind === "union" && type.types.some(arm =>
    arm.kind === "undefined" || (arm.kind === "literal" && arm.value === null));
}

function presentSourceType(type) {
  if (type.kind !== "union") return type;
  const present = type.types.filter(arm =>
    arm.kind !== "undefined" && !(arm.kind === "literal" && arm.value === null));
  assert.equal(present.length, 1, "one exact present native kind remains");
  return present[0];
}
