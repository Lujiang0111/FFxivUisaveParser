#include "byte_reader.h"
#include <string.h>

ByteReader::ByteReader(const uint8_t *data, size_t size, size_t offset) :
    data_(data),
    size_(size),
    offset_(offset)
{

}

ByteReader::~ByteReader()
{

}

bool ByteReader::CanRead(size_t size) const
{
    if ((size_ > 0) && (offset_ + size > size_))
    {
        return false;
    }
    return true;
}

uint64_t ByteReader::Read(size_t size)
{
    uint64_t value = 0;
    uint64_t unit = 1;
    while (size > 0)
    {
        value += (data_[offset_] * unit);
        ++offset_;
        unit <<= 8;
        --size;
    }
    return value;
}

void ByteReader::ReadArray(size_t size, std::vector<uint8_t> &dst)
{
    dst.resize(size);
    memcpy(&dst[0], &data_[0] + offset_, size);
    offset_ += size;
}
