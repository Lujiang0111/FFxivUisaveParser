# FFxivUisaveParser

解析FF14账户目录下的UISAVE.DAT文件（C#练习作品）

> 数据格式翻译自：<https://github.com/PunishedPineapple/UISAVE_Reader>

## UISAVE.DAT数据格式

### 原始数据格式

+ 数据按小端字节序存储

```rfc
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+                  file format version(possibly)                +
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                        encrypt length                         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           unknown                             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+                        encrypted data                         +
|                                                               |
```

+ encrypt length: 加密数据长度，单位byte
+ encrypted data：加密数据
  + 解密方式：对每个字节 xor 0x31后得到解密数据（decrypted data）

### decrypted data格式

+ 数据按小端字节序存储

```rfc
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+                           unknown                             +
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+                          Content ID                           +
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+                           sections                            +
|                                                               |
```

### sections格式

```rfc
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|  16-byte  |                 |  4-byte   |  16-byte  |
+  section  +     section     + trailing  +  section  +  ...
|  header   |      data       |   data    |  header   |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
```

1. section header

    ```rfc
    0                   1                   2                   3
    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    |        section index          |            unknown            |
    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    |                            unknown                            |
    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    |                      section data length                      |
    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    |                            unknown                            |
    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ```

    + section index：section索引编号，对应一种section类型
      + section index对应的类型
        | section index | 类型 | 含义 |
        | ---- | ---- | ---- |
        | 0x00 | LETTER.DAT | 邮件历史 |
        | 0x01 | RETTASK.DAT | |
        | 0x02 | FLAGS.DAT | |
        | 0x03 | RCFAV.DAT | |
        | 0x04 | UIDATA.DAT | 社交历史 |
        | 0x05 | TLPH.DAT | 传送历史 |
        | 0x06 | ITCC.DAT | |
        | 0x07 | PVPSET.DAT | |
        | 0x08 | EMTH.DAT | |
        | 0x09 | MNONLST.DAT | |
        | 0x0A | MUNTLST.DAT | |
        | 0x0B | EMJ.DAT | |
        | 0x0C | AOZNOTE.DAT | |
        | 0x0D | CWLS.DAT | 跨服通讯贝 |
        | 0x0E | ACHVLST.DAT | |
        | 0x0F | GRPPOS.DAT | |
        | 0x10 | CRAFT.DAT | |
        | 0x11 | FMARKER.DAT | 场地标点 |
    + section data length：section data的长度，单位byte（不包括section header)

2. section data
    + FMARKER.DAT 场地标点信息

3. trailing data
    + 4 byte，似乎都为0
