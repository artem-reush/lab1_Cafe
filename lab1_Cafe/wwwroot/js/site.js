// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function addField() {
    // определяем контейнер для хранения полей с вопросами
    let container = document.getElementById("items");
    // получаем текущее количество input (полей для вопросов)
    let fieldCount = container.getElementsByTagName("input").length;
    // увеличиваем Id для нового поля
    let nextFieldId = fieldCount;

    // здесь добавляем элемент, который будет хранить input (в моем случае, у вас может быть по другому или вообще не быть его)
    let div = document.createElement("div");
    div.setAttribute("class", "item");

    let div1 = document.createElement("div");
    div1.setAttribute("class", "form-group");

    let label1 = document.createElement("label");
    label1.textContent = "Блюдо: ";
    let select = document.createElement("select");
    select.setAttribute("class", "custom-select");
    select.setAttribute("name", "position[" + nextFieldId + "].Dish");
    let selectContainer = document.getElementById("select-item");
    let options = selectContainer.getElementsByTagName("option");
    for (var i = 0; i < options.length; i++) {
        let option = document.createElement("option");
        option.setAttribute("value", options[i].getAttribute("value"));
        option.textContent = options[i].textContent;
        select.appendChild(option);
    }
    div1.appendChild(label1);
    div1.appendChild(select);

    let div2 = document.createElement("div");
    div2.setAttribute("class", "form-group");
    let label2 = document.createElement("label");
    label2.textContent = "Количество: ";
    let input = document.createElement("input");
    input.setAttribute("class", "form-control");
    input.setAttribute("type", "text");
    input.setAttribute("name", "position[" + nextFieldId + "].Count");
    input.setAttribute("pattern", "[1-9][0-9]*");
    input.setAttribute("required", "true");
    div2.appendChild(label2);
    div2.appendChild(input);
    let hr = document.createElement("hr");

    // добавляем <div class="form-group"><input ... /></div> в главный контейнер
    div.appendChild(div1);
    div.appendChild(div2);
    div.appendChild(hr);
    container.appendChild(div);
}

function removeField() {
    // определяем контейнер для хранения полей с вопросами
    let container = document.getElementById("items");
    // получаем текущее количество input (полей для вопросов)
    let items = container.getElementsByClassName("item");
    let count = items.length;
    if (count > 1) {
        items[count - 1].parentNode.removeChild(items[count - 1]);
    }
}