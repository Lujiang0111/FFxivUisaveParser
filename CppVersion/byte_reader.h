#ifndef FFXIV_UISAVE_PARSER_BYTE_READER_H_
#define FFXIV_UISAVE_PARSER_BYTE_READER_H_

#include <stdint.h>
#include <vector>

class ByteReader
{
public:
    ByteReader(const uint8_t *data, size_t size = 0, size_t offset = 0);
    virtual ~ByteReader();

    bool CanRead(size_t size) const;
    uint64_t Read(size_t size);
    void ReadArray(size_t size, std::vector<uint8_t> &dst);

private:
    const uint8_t *data_;
    size_t size_;
    size_t offset_;
};

#endif // !FFXIV_UISAVE_PARSER_BYTE_READER_H_
