/**
 * Removes the specified lesson from the timetable (clears the lesson's cell)
 * @param {Number} lesson The index of lesson to remove
*/
function onLessonRemoved(lesson) {
    $('#cell-' + lesson).html('');
}

function onLessonClassSelected(lesson) {
    var selected = $('#select-' + lesson).val();
    var selectedParts = selected.split('-', 3);

    if (selectedParts.length > 1) {
        var name = selectedParts[0];
        var teacher = selectedParts[1];
        var roomIndex = name.length + teacher.length + 2;
        var room = '';
        if (selectedParts.length > 2)
            room = selected.substr(roomIndex, selected.length - roomIndex);

        $('#name-' + lesson).val(name);
        $('#teacher-' + lesson).val(teacher);
        $('#room-' + lesson).val(room);

        console.log('Lesson: ' + lesson + ' Name = ' + name + ' Teacher = ' + teacher + ' Room = ' + room);
    }
}

function onChangeRadioButton() {
    if (!('#semester-radio-1').prop('checked')) {

    }
}
