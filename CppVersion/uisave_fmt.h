#ifndef FFXIV_UISAVE_PARSER_UISAVE_FMT_H_
#define FFXIV_UISAVE_PARSER_UISAVE_FMT_H_

#include <string>
#include <memory>
#include "byte_reader.h"

class UisaveFmt
{
public:
    UisaveFmt() = delete;
    UisaveFmt(const UisaveFmt &) = delete;
    UisaveFmt &operator=(const UisaveFmt &) = delete;

    explicit UisaveFmt(const std::string &file_name);
    virtual ~UisaveFmt();

    bool Init();
    bool Parse();

private:
    void Decrypt(size_t size);
    void ParseWaymarkPresets();

private:
    std::string file_name_;
    std::vector<uint8_t> raw_data_;
    std::shared_ptr<ByteReader> raw_data_reader_;

    std::vector<uint8_t> decrypted_data_;
    std::shared_ptr<ByteReader> decrypted_data_reader_;

    std::vector<uint8_t> wp_section_data_;
    std::shared_ptr<ByteReader> wp_section_data_reader_;
};

#endif // !FFXIV_UISAVE_PARSER_UISAVE_FMT_H_
