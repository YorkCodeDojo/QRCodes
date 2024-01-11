use minifb::{Key, Window, WindowOptions};

use maths_rs::prelude::*;

type Rgb32 = u32;
type Vector2u = Vec2<u32>;
type Vector4u = Vec4<u32>;

// Lookup tables for indices around the QR code tiles
const UP_INDICES_4: &[(i32, i32)] = &[(1, 1), (0, 1), (1, 0), (0, 0)];

const UP_INDICES_8: &[(i32, i32)] = &[
    (1, 3),
    (0, 3),
    (1, 2),
    (0, 2),
    (1, 1),
    (0, 1),
    (1, 0),
    (0, 0),
];

const DOWN_INDICES_8: &[(i32, i32)] = &[
    (1, 0),
    (0, 0),
    (1, 1),
    (0, 1),
    (1, 2),
    (0, 2),
    (1, 3),
    (0, 3),
];

const LEFT_INDICES_8: &[(i32, i32)] = &[
    (3, 1),
    (2, 1),
    (3, 0),
    (2, 0),
    (1, 1),
    (0, 1),
    (1, 0),
    (0, 0),
];

const LEFT_UP_INDICES_8: &[(i32, i32)] = &[
    (3, 0),
    (2, 0),
    (3, 1),
    (2, 1),
    (1, 1),
    (0, 1),
    (1, 0),
    (0, 0),
];

// Paths around the QR code, to each top-left pixel
const WINDING: &[(i32, i32, &[(i32, i32)])] = &[
    (0, 0, UP_INDICES_8),
    (-2, -2, LEFT_INDICES_8),
    (0, 2, DOWN_INDICES_8),
    (0, 4, DOWN_INDICES_8),
    (-2, 4, LEFT_UP_INDICES_8),
    (0, -4, UP_INDICES_8),
    (0, -4, UP_INDICES_8),
    (-2, -2, LEFT_INDICES_8),
    (0, 2, DOWN_INDICES_8),
    (0, 4, DOWN_INDICES_8),
    (-2, 4, LEFT_UP_INDICES_8),
    (0, -4, UP_INDICES_8),
];

// Window output buffer
struct WindowBuffer {
    size: Vector2u,
    buffer: Vec<u32>,
}

// Fill a square in output buffer
fn fill_square(output: &mut WindowBuffer, rc: &Vector4u, col: &Rgb32) {
    for yy in 0..rc.w {
        for xx in 0..rc.z {
            let index = (rc.y + yy) * output.size.y + (rc.x + xx);

            output.buffer[index as usize] = *col;
        }
    }
}

// Write to the screen buffer, enlarging the code, putting it in the center, with an outline
fn qr_to_buffer(qr: &QR, output: &mut WindowBuffer) {
    let qr_width = qr.data[0].len() as u32;
    let qr_height = qr.data.len() as u32;

    // Calculate optimum square size for the window
    let square_size: u32 =
        (min(output.size.x, output.size.y) / max(qr_width as u32, qr_height as u32)) - 2;

    // Offset to the center
    let mut offset: Vector2u = Vector2u::new(
        output.size.x - (square_size * qr_width as u32),
        output.size.y - (square_size * qr_height as u32),
    );
    offset /= 2;

    // Draw the squares
    for y in 0..qr_height {
        for x in 0..qr_width {
            let mut col: Rgb32 = 0xFFFFFFFF;

            // Black entries, black
            if qr.data[y as usize][x as usize] == 1 {
                col = 0x0;
            }

            fill_square(
                output,
                &Vector4u::new(
                    x * square_size + offset.x,
                    y * square_size + offset.y,
                    square_size,
                    square_size,
                ),
                &col,
            );
        }
    }
}

#[derive(Debug)]
struct QR {
    width: u32,
    height: u32,
    data: Vec<Vec<u8>>,
    version: u32,
    mask_type: u8,
    name: String,
}

fn read_cell(data: &Vec<Vec<u8>>, start_coord: &(i32, i32), indices: &[(i32, i32)]) -> u8 {
    let mut number = 0;

    for index in indices {
        let x = (start_coord.0 + index.0) as usize;
        let y = (start_coord.1 + index.1) as usize;
        number = (number << 1) | data[y][x];
    }
    number
}

// Read the QR code
fn read_qr() -> QR {
    let mut data = include_str!("qr.txt")
        .lines()
        .map(|l| {
            l.as_bytes()
                .iter()
                .map(|b| if *b == b'B' { 1 } else { 0 })
                .collect()
        })
        .collect::<Vec<Vec<u8>>>();

    let width = data[0].len() as u32;
    let height = data.len() as u32;
    let version = (data[0].len() as u32 - 17) / 4;
    let mask_bits = data[8][0..5].to_vec();

    // Calculate the mask bits
    let mut mask = 0;
    for bit in mask_bits.iter() {
        mask = mask << 1 | bit
    }

    let xor_mask = 0b10101;
    let mask_type = mask ^ xor_mask;

    for row in 0..height {
        for col in 0..width {
            //let index = (((row * col) % 2) + ((row * col) % 3)) % 2;
            let index = (((row * col) % 3) + (row * col)) % 2;
            if index == 0 {
                if data[row as usize][col as usize] == 0 {
                    data[row as usize][col as usize] = 1;
                } else {
                    data[row as usize][col as usize] = 0;
                }
            }
        }
    }

    let mut current_x = (width - 2) as i32;
    let mut current_y = (height - 2) as i32;

    // Read encoding and length
    let _encoding = read_cell(&mut data, &(current_x, current_y), &UP_INDICES_4);
    current_y -= 4;
    let _length = read_cell(&mut data, &(current_x, current_y), &UP_INDICES_8);
    current_y -= 4;

    // Walk around the cells
    let mut name: String = "".to_string();
    for wind in WINDING {
        current_y += wind.1;
        current_x += wind.0;
        name.push(read_cell(&mut data, &(current_x, current_y), wind.2) as char);
    }

    QR {
        width,
        height,
        data,
        version,
        mask_type,
        name,
    }
}

fn main() {
    // Make a 512x512 window
    let window_size = Vector2u::new(512, 512);
    let mut window = Window::new(
        "QR Codes",
        window_size.x as usize,
        window_size.y as usize,
        WindowOptions::default(),
    )
    .unwrap_or_else(|e| {
        panic!("{}", e);
    });

    window.limit_update_rate(Some(std::time::Duration::from_millis(100)));

    let mut output = WindowBuffer {
        size: window_size,
        buffer: vec![0xFFAAAAAA; (window_size.x * window_size.y) as usize],
    };

    let qr = read_qr();
    dbg!(&qr.version);
    dbg!(&qr.width);
    dbg!(&qr.height);
    dbg!(&qr.mask_type);
    dbg!(&qr.name);

    qr_to_buffer(&qr, &mut output);

    // Update the window contents
    window
        .update_with_buffer(
            &output.buffer,
            window_size.x as usize,
            window_size.y as usize,
        )
        .unwrap();

    // Window loop
    while window.is_open() && !window.is_key_down(Key::Escape) {
        window
            .update_with_buffer(
                &output.buffer,
                window_size.x as usize,
                window_size.y as usize,
            )
            .unwrap();
    }
}
