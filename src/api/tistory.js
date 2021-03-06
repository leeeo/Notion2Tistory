const request = require("request");
const { BrowserWindow } = require("electron").remote;

const AUTHORIZE_URL = "https://www.tistory.com/oauth/authorize";
const CLIENT_ID = "ff97cbe9c5811dbf23fc9f9622f3d675";
const CLIENT_SECRET =
    "ff97cbe9c5811dbf23fc9f9622f3d6758fd84a4f15439f76e49ccc03b01e38a425ac1dbc";
const REDIRECT_URL = "http://boltlessengineer.tistory.com";

// Implicit 방식
// 티스토리에서 더이상 지원하지 않음
const IMPLICIT_OAUTH = `${AUTHORIZE_URL}?client_id=${CLIENT_ID}&redirect_uri=${REDIRECT_URL}&response_type=token`;

// Authorization code 방식
const CODE_OAUTH = `${AUTHORIZE_URL}?client_id=${CLIENT_ID}&redirect_uri=${REDIRECT_URL}&response_type=code`;

const getAccessCode = () => {
    return new Promise((res, rej) => {
        const authWin = new BrowserWindow({
            width: 1000,
            height: 800,
            show: false,
            "node-integration": false,
        });

        authWin.setMenu(null);

        const handleNavigation = (url) => {
            console.log(url);
            // 원하던대로 갔으면 그대로 실행하고
            // 이상한대로 갔으면(로그인창 조차 아닌 경우) 창 닫기
            authWin.webContents.executeJavaScript(
                "(" + logoutBtn.toString() + ")()"
            );
            const raw_code = /\?code=([^&]*)/.exec(url);
            const accessCode =
                raw_code && raw_code.length > 1 ? raw_code[1] : null;

            if (accessCode) {
                console.log(`access code : ${accessCode}`);
                authWin.close();
                res(accessCode);
            } else {
                authWin.show();
            }
        };
        authWin.loadURL(CODE_OAUTH);

        authWin.webContents.on("did-navigate", (event, url) => {
            console.log("did-navigate");
            handleNavigation(url);
        });

        authWin.webContents.on(
            "did-get-redirect-request",
            (event, oldUrl, newUrl) => {
                console.log("did-get-redirect-request");
                handleNavigation(newUrl);
            }
        );
    });
};

const getTokenFromCode = (accessCode) => {
    return new Promise((res, rej) => {
        const options = {
            uri: "https://www.tistory.com/oauth/access_token",
            qs: {
                client_id: CLIENT_ID,
                client_secret: CLIENT_SECRET,
                redirect_uri: REDIRECT_URL,
                code: accessCode,
                grant_type: "authorization_code",
            },
        };
        request(options, (err, response, body) => {
            const raw_token = /access_token=([^&]*)/.exec(body);
            console.log(raw_token);
            const accessToken =
                raw_token && raw_token.length > 1 ? raw_token[1] : null;
            if (accessToken) {
                res(accessToken);
            } else {
                console.log(err);
                console.log(response);
                console.log(body);
                rej("error");
            }
        });
    });
};

const getAccessToken = async () => {
    const code = await getAccessCode();
    console.log(code);
    return getTokenFromCode(code);
};

const getAccessToken_Implicit = () => {
    // [ToDo]
    // 나중에 로그인 창(브라우저) 띄우는건 index.js로 옮기거나
    // window 창만 관리하는 js파일을 따로 만들것!
    return new Promise((res, rej) => {
        const authWin = new BrowserWindow({
            width: 1000,
            height: 800,
            show: false,
            "node-integration": false,
        });

        const handleNavigation = (url) => {
            const raw_token = /\#access_token=([^&]*)/.exec(url);
            const accessToken =
                raw_token && raw_token.length > 1 ? raw_token[1] : null;

            if (accessToken) {
                console.log(`access_token : ${accessToken}`);
                authWin.close();
                res(accessToken);
            } else {
                authWin.show();
            }
        };
        authWin.loadURL(TISTORY_OAUTH);

        authWin.webContents.on("did-navigate", (event, url) => {
            console.log("did-navigate");
            handleNavigation(url);
        });

        authWin.webContents.on(
            "did-get-redirect-request",
            (event, oldUrl, newUrl) => {
                console.log("did-get-redirect-request");
                handleNavigation(newUrl);
            }
        );
    });
};

const logoutBtn = () => {
    const btn = document.createElement("div");
    btn.style =
        "display: flex; width: 60px; height: 60px; position: fixed; right: 20px; bottom: 20px; border-radius: 50%; background-color: rgb(240, 240, 240); align-items: center; justify-content: center";
    btn.innerHTML = "logout";
    btn.addEventListener("click", () => {
        window.location.href = "https://www.tistory.com/auth/logout";
    });
    document.body.appendChild(btn);
};

const findCategory = async (usr, categoryName) => {
    const query = {
        access_token: usr.accessToken,
        output: "json",
        blogName: usr.blogName,
    };
    console.log(query);
    const myRequest = () => {
        return new Promise((res, rej) => {
            request(
                {
                    uri: "https://www.tistory.com/apis/category/list",
                    method: "GET",
                    qs: query,
                },
                (err, response, body) => {
                    console.log(err);
                    console.log(response);
                    console.log(body);
                    const resBody = JSON.parse(body).tistory;
                    if (resBody.status != 200) {
                        rej(
                            `Error [${resBody.status}] : ${resBody.error_message}`
                        );
                    } else {
                        res(resBody.item.categories);
                    }
                }
            );
        });
    };
    const categoryList = await myRequest();
    console.log(categoryList);
    if (categoryList) {
        const foundList = categoryList.filter(
            (category) => category.name === categoryName
        );
        if (foundList.length > 0) {
            return foundList[0].id.toString();
        } else {
            return "0";
        }
    }
};

const uploadData = (usr, data) => {
    // data : { value: buffer, options: { filename, } }
    console.log(data);
    const requestUri = "https://www.tistory.com/apis/post/attach";
    console.log(usr);
    const formData = {
        access_token: usr.accessToken,
        output: "json",
        blogName: usr.blogName,
        uploadedfile: data,
    };
    return new Promise((resolve, reject) => {
        request.post(
            { url: requestUri, formData: formData },
            (err, res, body) => {
                const resBody = JSON.parse(body).tistory;
                if (resBody.status != 200) {
                    console.log(
                        `Error [${resBody.status}] : ${resBody.error_message}`
                    );
                    reject(resBody);
                } else {
                    console.log(resBody);
                    resolve(resBody);
                }
            }
        );
    });
};

const uploadImage = async (usr, data) => {
    console.log(data);
    const resBody = await uploadData(usr, data);
    const url = resBody.url;
    const replacer = getImageReplacer(url);
    return replacer;
};

const getImageReplacer = (url) => {
    const fileId = url.slice(url.lastIndexOf("/") + 1, url.length - 4);
    const newUrl = `https://t1.daumcdn.net/cfile/tistory/${fileId}?original`;
    const replacer = `[##_Image|t/cfile@${fileId}|alignCenter|data-origin-width="0" data-origin-height="0" data-ke-mobilestyle="widthContent"|||_##]`;

    return { replacer, url: newUrl };
};

const dateToTimestamp = (date) => {
    if (!date) {
        return null;
    }
    console.log(`Publish Date : ${date}`);
    const timestamp = Date.parse(date.substring(1)) / 1000;
    console.log(timestamp);
    return timestamp.toString();
};

const uploadPost = async (usr, notionPage) => {
    const categoryId = await findCategory(usr, notionPage.Category);
    console.log(categoryId);
    console.log("start");
    const publishTimestamp = dateToTimestamp(notionPage.PublishDate);
    const vis = notionPage.Visibility.toLowerCase();
    let visibility = "0"; //private
    switch (vis) {
        case "protected":
            visibility = "1";
            break;
        case "public":
            visibility = "3";
            break;
    }

    let form = {
        access_token: usr.accessToken,
        output: "json",
        blogName: usr.blogName,
        title: notionPage.Title,
        content: notionPage.content.outerHTML,
        visibility: visibility,
        category: categoryId,
        tag: notionPage.Tag.join(","),
        acceptComment: notionPage.Comment ? "1" : "0",
        password: notionPage.Password,
    };

    if (publishTimestamp) {
        form.published = publishTimestamp;
    }
    console.log(form);

    return new Promise((resolve, reject) => {
        request(
            {
                uri: "https://www.tistory.com/apis/post/write",
                method: "POST",
                form: form,
            },
            (err, res, body) => {
                const resBody = JSON.parse(body).tistory;
                if (resBody.status != 200) {
                    const error = `Error [${resBody.status}] : ${resBody.error_message}`;
                    console.log(error);
                    reject(error);
                } else {
                    console.log(resBody);

                    const postUrl = resBody.url;
                    resolve(postUrl);
                }
            }
        );
    });
};

module.exports = {
    getAccessToken,
    uploadPost,
    uploadData,
    uploadImage,
};
