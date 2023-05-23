package com.lemuelinchrist.android.hymns.utils.Networks;

import com.fasterxml.jackson.annotation.JsonAlias;

public class HymnYT {
    @JsonAlias({ "hymnid" })
    public String unique_id;
    @JsonAlias({ "link" })
    public String youtube_link;
}
